using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TRENZ.Extensions.Infisical;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, Action<InfisicalConfigurationOptions> configure)
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
        options.PollingInterval = infisicalConfigSection["PollingInterval"] is { } pollingInterval ? long.Parse(pollingInterval) : null;

        configure(options);

        return builder.Add(new InfisicalConfigurationSource(options));
    }

    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, IHostEnvironment environment, Action<InfisicalConfigurationOptions>? configure = null)
    {
        return builder.AddInfisical(c =>
        {
            c.EnvironmentName = environment.EnvironmentName;

            configure?.Invoke(c);
        });
    }
}
