using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddInfisicalConfiguration(this IHostApplicationBuilder builder,
        Action<InfisicalConfigurationOptions>? configure = null) => AddInfisicalConfiguration(builder, null, configure);

    public static IHostApplicationBuilder AddInfisicalConfiguration(this IHostApplicationBuilder builder,
        ILoggerFactory? loggerFactory, Action<InfisicalConfigurationOptions>? configure = null)
    {
        builder.Configuration.AddInfisical(builder.Environment, loggerFactory, configure);

        return builder;
    }
}
