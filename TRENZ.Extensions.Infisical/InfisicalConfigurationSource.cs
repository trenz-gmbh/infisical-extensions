using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationSource(InfisicalConfigurationOptions options, ILoggerFactory? loggerFactory = null) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var providerLogger = loggerFactory?.CreateLogger<InfisicalConfigurationProvider>();
        var clientWrapperLogger = loggerFactory?.CreateLogger<DefaultInfisicalClientWrapper>();

        var defaultClientWrapper = new DefaultInfisicalClientWrapper(clientWrapperLogger, options);

        return new InfisicalConfigurationProvider(providerLogger, options, defaultClientWrapper);
    }
}
