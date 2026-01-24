using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder,
        Action<InfisicalConfigurationOptions> configure) => AddInfisical(builder, (ILoggerFactory?)null, configure);

    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, ILoggerFactory? loggerFactory,
        Action<InfisicalConfigurationOptions> configure)
    {
        var options = new InfisicalConfigurationOptions();

        var tempConfig = builder.Build();
        var infisicalConfigSection = tempConfig.GetRequiredSection("Infisical");
        options.ClientId = infisicalConfigSection["ClientId"];
        options.ClientSecret = infisicalConfigSection["ClientSecret"];
        options.SiteUrl = infisicalConfigSection["SiteUrl"];
        options.ProjectId = infisicalConfigSection["ProjectId"];
        options.AccessToken = infisicalConfigSection["AccessToken"];
        options.CacheTtl = infisicalConfigSection["CacheTtl"] is { } cacheTtl ? long.Parse(cacheTtl) : null;
        options.UserAgent = infisicalConfigSection["UserAgent"];
        options.DisableMappingToInfisicalStandardEnvironments = infisicalConfigSection["DisableMappingToInfisicalStandardEnvironments"] is { } disableMapping 
            ? bool.Parse(disableMapping)
            : null;
        options.PollingInterval = infisicalConfigSection["PollingInterval"] is { } pollingInterval
            ? long.Parse(pollingInterval)
            : null;

        configure(options);

        return builder.Add(new InfisicalConfigurationSource(options, loggerFactory));
    }

    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, IHostEnvironment environment,
        Action<InfisicalConfigurationOptions>? configure = null) => AddInfisical(builder, environment, null, configure);

    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, IHostEnvironment environment,
        ILoggerFactory? loggerFactory, Action<InfisicalConfigurationOptions>? configure = null)
    {
        return builder.AddInfisical(loggerFactory, c =>
        {
            c.EnvironmentName = environment.EnvironmentName.ToLowerInvariant();
            if (string.IsNullOrEmpty(c.EnvironmentName)) {
                c.EnvironmentName ??= "development";
            }
            
            if (!c.DisableMappingToInfisicalStandardEnvironments.GetValueOrDefault())
            {
                switch (c.EnvironmentName.ToLowerInvariant())
                {
                    case "development":
                        c.EnvironmentName = "dev";
                        break;
                    case "production":
                        c.EnvironmentName = "prod";
                        break;
                }
            }

            configure?.Invoke(c);
        });
    }
}
