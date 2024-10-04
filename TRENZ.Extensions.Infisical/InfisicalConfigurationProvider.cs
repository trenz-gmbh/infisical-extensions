using System.Collections.Frozen;
using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly Lazy<InfisicalClient> lazyClient;

    private readonly Timer? checkForChangesTimer;

    private readonly InfisicalConfigurationOptions options;

    private IDictionary<string, SecretElement> secrets = new Dictionary<string, SecretElement>();

    private CancellationTokenSource reloadTokenSource = new();

    public InfisicalConfigurationProvider(InfisicalConfigurationOptions options)
    {
        this.options = options;

        lazyClient = new Lazy<InfisicalClient>(() =>
        {
            if (string.IsNullOrEmpty(options.ClientId))
                throw new InfisicalException("ClientId is not set.");

            if (string.IsNullOrEmpty(options.ClientSecret))
                throw new InfisicalException("ClientSecret is not set.");

            if (string.IsNullOrEmpty(options.SiteUrl))
                throw new InfisicalException("SiteUrl is not set.");

            var settings = new ClientSettings
            {
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
                SiteUrl = options.SiteUrl,
                UserAgent = options.UserAgent!,
                CacheTtl = options.CacheTtl,
                AccessToken = options.AccessToken!,
            };

            return new InfisicalClient(settings);
        });

        if (options.PollingInterval is { } pollingInterval)
        {
            checkForChangesTimer = new Timer(CheckForChanges, null, pollingInterval, pollingInterval);
        }
    }

    private string EnvironmentName => options.EnvironmentName.ToLowerInvariant();

    private InfisicalClient Client => lazyClient.Value;

    private TimeSpan LoadTimeout
    {
        get
        {
            return options.LoadTimeout switch
            {
                null => TimeSpan.FromSeconds(5),
                < 0 => Timeout.InfiniteTimeSpan,
                _ => TimeSpan.FromMilliseconds(options.LoadTimeout.Value),
            };
        }
    }

    private void CheckForChanges(object? state)
    {
        LoadSecretsWithTimeout(newSecrets =>
        {
            if (!CheckHasChanged(newSecrets))
                return;

            var previousToken = reloadTokenSource;

            Load();
            reloadTokenSource = new CancellationTokenSource();

            previousToken.Cancel();
        }, LoadTimeout);
    }

    public bool TryGet(string key, out string? value)
    {
        if (secrets.TryGetValue(key, out var secret))
        {
            value = secret.SecretValue;

            return true;
        }

        value = null;

        return false;
    }

    public void Set(string key, string? value)
    {
        throw new NotSupportedException("Setting values in infisical is not supported.");
    }

    public IChangeToken GetReloadToken() => new CancellationChangeToken(reloadTokenSource.Token);

    public void Load()
    {
        LoadSecretsWithTimeout(s => secrets = s, LoadTimeout);
    }

    private bool CheckHasChanged(IDictionary<string, SecretElement> newSecrets)
    {
        var oldSecrets = secrets;

        if (newSecrets.Count != oldSecrets.Count)
            return true;

        foreach (var (key, value) in newSecrets)
        {
            if (!oldSecrets.TryGetValue(key, out var oldValue))
                return true;

            if (value.SecretValue != oldValue.SecretValue)
                return true;
        }

        return false;
    }

    private void LoadSecretsWithTimeout(Action<IDictionary<string, SecretElement>> callback, TimeSpan timeout)
    {
        var task = Task.Run(() => callback(LoadSecrets()));
        if (!task.Wait(timeout))
            throw new InfisicalException("Timeout while loading secrets.");
    }

    private FrozenDictionary<string, SecretElement> LoadSecrets()
    {
        try
        {
            return LoadSecretsWithRetries();
        }
        catch (InfisicalException e)
        {
            throw new InfisicalException("Failed to load secrets.", e);
        }
    }

    private FrozenDictionary<string, SecretElement> LoadSecretsWithRetries()
    {
        var projectId = options.ProjectId;
        if (string.IsNullOrEmpty(projectId))
            throw new InfisicalException("ProjectId is not set.");

        var request = new ListSecretsOptions
        {
            Environment = EnvironmentName,
            ProjectId = projectId,
        };

        const int maxRetries = 10;
        var retries = 0;
        while (true)
        {
            try
            {
                return Client.ListSecrets(request).ToFrozenDictionary(s => s.SecretKey);
            }
            catch (InfisicalException e)
            {
                retries++;
                if (retries >= maxRetries)
                    throw new InfisicalException("Max retries exceeded.", e);

                // back off exponentially but at least 50ms
                var backoff = 50 + 5 * Math.Pow(2, retries);
                Thread.Sleep((int)backoff);
            }
        }
    }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        parentPath ??= string.Empty;

        foreach (var earlierKey in earlierKeys)
        {
            // omit all infisical keys
            if (parentPath.Length == 0 && earlierKey.StartsWith("Infisical"))
                continue;

            yield return earlierKey;
        }

        var returnedKeys = new HashSet<string>();
        foreach (var key in secrets.Keys)
        {
            if (!key.StartsWith(parentPath))
                continue;

            var remainingPath = key[parentPath.Length..];
            if (remainingPath.StartsWith(ConfigurationPath.KeyDelimiter))
                remainingPath = remainingPath[1..];

            if (remainingPath.Length == 0)
                continue;

            var currentKey = remainingPath.Split(ConfigurationPath.KeyDelimiter).First();

            if (returnedKeys.Add(currentKey))
                yield return currentKey;
        }
    }

    public void Dispose()
    {
        checkForChangesTimer?.Dispose();

        if (!lazyClient.IsValueCreated)
            return;

        lazyClient.Value.Dispose();
    }
}
