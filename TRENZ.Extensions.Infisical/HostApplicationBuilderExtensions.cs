using Microsoft.Extensions.Hosting;

namespace TRENZ.Extensions.Infisical;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfisicalConfiguration(this IHostApplicationBuilder builder, Action<InfisicalConfigurationOptions>? configure = null)
    {
        builder.Configuration.AddInfisical(builder.Environment, configure);

        return builder;
    }
}
