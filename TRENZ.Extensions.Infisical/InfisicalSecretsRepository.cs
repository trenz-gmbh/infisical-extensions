using System.Collections.Frozen;
using Infisical.Sdk;
using Infisical.Sdk.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public class InfisicalSecretsRepository(
    ILogger<InfisicalSecretsRepository>? logger,
    InfisicalConfigurationOptions options
) : ISecretsRepository, IDisposable
{
    internal static InfisicalConfigurationOptions ValidateSettingsFromOptions(InfisicalConfigurationOptions options)
    {
        options.EnvironmentName ??= "development";
        if (!options.DisableMappingToInfisicalStandardEnvironments.GetValueOrDefault())
        {
            switch (options.EnvironmentName.ToLowerInvariant())
            {
                case "development":
                    options.EnvironmentName = "dev";
                    break;
                case "production":
                    options.EnvironmentName = "prod";
                    break;
            }
        }

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

    private readonly InfisicalClient? _client = GetClient(logger, options);

    public void Dispose()
    {
        if (logger is IDisposable d)
            d.Dispose();

        GC.SuppressFinalize(this);
    }

    public IDictionary<string, Secret>? GetAllSecrets()
    {
        var request = new ListSecretsOptions
        {
            EnvironmentSlug = options.EnvironmentName,
            ProjectId = options.ProjectId,
        };

        const int maxRetries = 10;
        var retries = 0;
        while (true)
        {
            try
            {
                if (_client != null)
                {
                var results = _client.Secrets().ListAsync(request).GetAwaiter().GetResult();
                return results.ToFrozenDictionary(s => s.SecretKey);
            }
            }
            catch (InfisicalException e)
            {
                logger?.LogInformation(e, "Failed to load secrets");

                retries++;
                if (retries >= maxRetries)
                {
                    logger?.LogError(e, "Max retries exceeded");

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
