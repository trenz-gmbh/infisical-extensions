using System.Collections.Frozen;
using Infisical.Sdk;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public class InfisicalSecretsRepository(
    ILogger<InfisicalSecretsRepository>? logger,
    InfisicalConfigurationOptions options
) : ISecretsRepository, IDisposable
{
    internal static ClientSettings CreateSettingsFromOptions(InfisicalConfigurationOptions options)
    {
        if (string.IsNullOrEmpty(options.ProjectId))
            throw new InfisicalException("ProjectId is not set.");

        if (string.IsNullOrEmpty(options.ClientId))
            throw new InfisicalException("ClientId is not set.");

        if (string.IsNullOrEmpty(options.ClientSecret))
            throw new InfisicalException("ClientSecret is not set.");

        if (string.IsNullOrEmpty(options.SiteUrl))
            throw new InfisicalException("SiteUrl is not set.");

        if (!Uri.TryCreate(options.SiteUrl, UriKind.Absolute, out var givenSiteUrl))
            throw new InfisicalException("SiteUrl is not a valid URL");

        if (givenSiteUrl.Scheme != "https" && givenSiteUrl.Host != "localhost")
            throw new InfisicalException("SiteUrl must use HTTPS scheme");

        var sanitizedSiteUrl = new UriBuilder
            {
                Scheme = givenSiteUrl.Scheme,
                Host = givenSiteUrl.Host,
                Port = givenSiteUrl.Port,
            }
            .Uri
            .ToString()
            .TrimEnd('/'); // empty path results in trailing /

        var settings = new ClientSettings
        {
            ClientId = options.ClientId,
            ClientSecret = options.ClientSecret,
            SiteUrl = sanitizedSiteUrl,
            CacheTtl = options.CacheTtl,
#nullable disable
            // These properties are _not_ nullable in the Infisical SDK, but they _can_  be null in our config.
            // This means we intentionally suppress nullable warnings here
            UserAgent = options.UserAgent,
            AccessToken = options.AccessToken,
#nullable restore
        };

        return settings;
    }

    private readonly InfisicalClient client = new(CreateSettingsFromOptions(options));

    private readonly string projectId = options.ProjectId ?? throw new InfisicalException("ProjectId is not set.");

    private readonly string environmentSlug = options.EnvironmentName.ToLowerInvariant();

    public void Dispose()
    {
        client.Dispose();

        if (logger is IDisposable d)
            d.Dispose();

        GC.SuppressFinalize(this);
    }

    public IDictionary<string, SecretElement>? GetAllSecrets()
    {
        var request = new ListSecretsOptions
        {
            Environment = environmentSlug,
            ProjectId = projectId,
        };

        const int maxRetries = 10;
        var retries = 0;
        while (true)
        {
            try
            {
                return client.ListSecrets(request).ToFrozenDictionary(s => s.SecretKey);
            }
            catch (InfisicalException e) 
                when (e.InnerException != null && 
                      e.InnerException.Message.Contains("Error during GET request: Unexpected response: Unauthorized"))
            {
                logger?.LogError(e, "Failed to load secrets: check credentials to Infisical secrets instance.");

                return null;
            }
            catch (Exception e)
            {
                retries++;
                
                logger?.LogWarning(e, "Failed to load secrets, attempt #{Try}", retries);

                if (retries >= maxRetries)
                {
                    logger?.LogCritical(e, "Max retries exceeded");

                    return null;
                }
            }

            // can't use async here, see https://github.com/dotnet/runtime/issues/36018

            // back off exponentially but at least 50ms
            var backoff = 50 + 5 * Math.Pow(2, retries);
            Thread.Sleep((int)backoff);            
        }
    }
}
