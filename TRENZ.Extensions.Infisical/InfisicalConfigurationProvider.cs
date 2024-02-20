using System.Collections.Frozen;
using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationProvider(InfisicalConfigurationOptions options) : IConfigurationProvider, IDisposable
{
    private string ProjectId => options.ProjectId ?? throw new InfisicalException("ProjectId is not set.");

    private string EnvironmentName => options.EnvironmentName.ToLowerInvariant();

    private InfisicalClient Client => lazyClient.Value;

    private readonly Lazy<InfisicalClient> lazyClient = new(() =>
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

    private IDictionary<string, SecretElement> secrets = new Dictionary<string, SecretElement>();

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

    public IChangeToken GetReloadToken() => null!;

    public void Load()
    {
        var request = new ListSecretsOptions
        {
            Environment = EnvironmentName,
            ProjectId = ProjectId,
        };

        secrets = Client.ListSecrets(request).ToFrozenDictionary(s => s.SecretKey);
    }

    public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        parentPath ??= string.Empty;
        foreach (var key in secrets.Keys)
        {
            if (!key.StartsWith(parentPath))
                continue;

            yield return key[parentPath.Length..].Split(":").First();
        }
    }

    public void Dispose()
    {
        if (!lazyClient.IsValueCreated)
            return;

        lazyClient.Value.Dispose();
    }
}
