using System.Collections.Frozen;
using Infisical.Sdk;
using Infisical.Sdk.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public class InfisicalSecretsRepository(
    ILogger<InfisicalSecretsRepository>? logger,
    InfisicalConfigurationOptions options
) : ISecretsRepository
{
    internal static InfisicalConfigurationOptions ValidateSettingsFromOptions(InfisicalConfigurationOptions options)
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

        options.SiteUrl = new UriBuilder
        {
            Scheme = givenSiteUrl.Scheme,
            Host = givenSiteUrl.Host,
            Port = givenSiteUrl.Port,
        }
            .Uri
            .ToString()
            .TrimEnd('/'); // empty path results in trailing /

        return options;
    }

    private static InfisicalClient GetClient(ILogger? logger, InfisicalConfigurationOptions options)
    {
        options = ValidateSettingsFromOptions(options); // ensure valid options

        var sdkSettings = new InfisicalSdkSettingsBuilder()
            .WithHostUri(options.SiteUrl!)
            .Build();

        var client = new InfisicalClient(sdkSettings);

        try
        {
            logger?.LogDebug("Connecting to Infisical secrets instance at {Url}, environment {Environment}", 
                options.SiteUrl, options.EnvironmentName);

            client.Auth().UniversalAuth()
                .LoginAsync(options.ClientId!, options.ClientSecret!)
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to log into Infisical secrets instance.");
        }

        return client;
    }

    private readonly InfisicalClient client = GetClient(logger, options);

    private readonly string projectId = options.ProjectId ?? throw new InfisicalException("ProjectId is not set.");

    private readonly string environmentSlug = options.EnvironmentName.ToLowerInvariant();

    public IDictionary<string, Secret>? GetAllSecrets()
    {
        var request = new ListSecretsOptions
        {
            EnvironmentSlug = environmentSlug,
            ProjectId = projectId,
        };

        const int maxRetries = 10;
        var retries = 0;
        while (true)
        {
            try
            {
                return client.Secrets().ListAsync(request)
                    .GetAwaiter().GetResult().ToFrozenDictionary(s => s.SecretKey);
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
