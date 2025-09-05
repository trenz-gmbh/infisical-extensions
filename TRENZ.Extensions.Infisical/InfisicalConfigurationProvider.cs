using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly ILogger<InfisicalConfigurationProvider>? logger;

    private readonly ISecretsRepository client;

    private readonly Timer? checkForChangesTimer;

    private readonly InfisicalConfigurationOptions options;

    private IDictionary<string, SecretElement> secrets = new Dictionary<string, SecretElement>();

    private CancellationTokenSource reloadTokenSource = new();

    public InfisicalConfigurationProvider(ILogger<InfisicalConfigurationProvider>? logger,
        InfisicalConfigurationOptions options, ISecretsRepository client)
    {
        this.logger = logger;
        this.options = options;
        this.client = client;

        if (options.PollingInterval is { } pollingInterval)
        {
            checkForChangesTimer = new Timer(CheckForChanges, null, pollingInterval, pollingInterval);
        }
    }

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
            if (newSecrets == null) // secret loading failed, handle gracefully
                return;

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
        LoadSecretsWithTimeout(s =>
        {
            if (s == null)
                return;

            secrets = s;
        }, LoadTimeout);
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

    private void LoadSecretsWithTimeout(Action<IDictionary<string, SecretElement>?> callback, TimeSpan timeout)
    {
        try
        {
            var loadTask = Task.Run(() => client.GetAllSecrets());

            if (!loadTask.Wait(timeout))
            {
                logger?.LogWarning("Failed loading secrets (timed out after {Timeout})", timeout);

                return;
            }

            if (!loadTask.IsCompletedSuccessfully)
            {
                logger?.LogWarning(loadTask.Exception, "Failed loading secrets");

                return;
            }

            callback(loadTask.Result);
        }
        catch (Exception e)
        {
            logger?.LogWarning(e, "Failed loading secrets");
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

        reloadTokenSource.Dispose();

        if (logger is IDisposable d)
            d.Dispose();

        if (!secrets.IsReadOnly)
            secrets.Clear();

        GC.SuppressFinalize(this);
    }
}
