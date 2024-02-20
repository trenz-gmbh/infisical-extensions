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

        lazyClient = new(() =>
        {
            var settings = new ClientSettings
            {
                ClientId = options.ClientId ?? throw new InfisicalException("ClientId is not set."),
                ClientSecret = options.ClientSecret ?? throw new InfisicalException("ClientSecret is not set."),
                SiteUrl = options.SiteUrl ?? throw new InfisicalException("SiteUrl is not set."),
                UserAgent = options.UserAgent!,
                CacheTtl = options.CacheTtl,
                AccessToken = options.AccessToken!,
            };

            return new(settings);
        });

        if (options.PollingInterval is { } pollingInterval)
        {
            checkForChangesTimer = new(CheckForChanges, null, pollingInterval, pollingInterval);
        }
    }

    private string ProjectId => options.ProjectId ?? throw new InfisicalException("ProjectId is not set.");

    private string EnvironmentName => options.EnvironmentName.ToLowerInvariant();

    private InfisicalClient Client => lazyClient.Value;

    private void CheckForChanges(object? state)
    {
        if (!CheckHasChanged())
            return;

        var previousToken = reloadTokenSource;

        Load();
        reloadTokenSource = new();

        previousToken.Cancel();
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
        secrets = LoadSecrets();
    }

    private bool CheckHasChanged()
    {
        var newSecrets = LoadSecrets();
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

    private FrozenDictionary<string, SecretElement> LoadSecrets()
    {
        var request = new ListSecretsOptions
        {
            Environment = EnvironmentName,
            ProjectId = ProjectId,
        };

        return Client.ListSecrets(request).ToFrozenDictionary(s => s.SecretKey);
    }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        parentPath ??= string.Empty;

        var returnedKeys = new HashSet<string>();
        foreach (var key in secrets.Keys)
        {
            if (!key.StartsWith(parentPath))
                continue;

            var remainingPath = key[parentPath.Length..];
            if (remainingPath.StartsWith(':'))
                remainingPath = remainingPath[1..];

            if (remainingPath.Length == 0)
                continue;

            var currentKey = remainingPath.Split(":").First();

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
