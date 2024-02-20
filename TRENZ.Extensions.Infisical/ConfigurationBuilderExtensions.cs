using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace TRENZ.Extensions.Infisical;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, Action<InfisicalConfigurationOptions> configure)
    {
        var options = new InfisicalConfigurationOptions();

        configure(options);

        return builder.Add(new InfisicalConfigurationSource(options));
    }

    public static IConfigurationBuilder AddInfisical(this IConfigurationBuilder builder, IHostEnvironment environment, Action<InfisicalConfigurationOptions>? configure = null)
    {
        return builder.AddInfisical(c =>
        {
            c.EnvironmentName = environment.EnvironmentName;

            var tempConfig = builder.Build();
            var infisicalConfigSection = tempConfig.GetRequiredSection("Infisical");
            c.ClientId = infisicalConfigSection["ClientId"];
            c.ClientSecret = infisicalConfigSection["ClientSecret"];
            c.SiteUrl = infisicalConfigSection["SiteUrl"];
            c.ProjectId = infisicalConfigSection["ProjectId"];
            c.AccessToken = infisicalConfigSection["AccessToken"];
            c.CacheTtl = infisicalConfigSection["CacheTtl"] is { } cacheTtl ? long.Parse(cacheTtl) : null;
            c.UserAgent = infisicalConfigSection["UserAgent"];
            c.PollingInterval = infisicalConfigSection["PollingInterval"] is { } pollingInterval ? long.Parse(pollingInterval) : null;

            configure?.Invoke(c);
        });
    }
}
