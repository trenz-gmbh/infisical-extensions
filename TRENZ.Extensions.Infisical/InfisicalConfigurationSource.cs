using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationSource(InfisicalConfigurationOptions options, ILoggerFactory? loggerFactory = null) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var providerLogger = loggerFactory?.CreateLogger<InfisicalConfigurationProvider>();
        var clientWrapperLogger = loggerFactory?.CreateLogger<InfisicalSecretsRepository>();

        var defaultClientWrapper = new InfisicalSecretsRepository(clientWrapperLogger, options);

        if (options.EnableUnderscoreToColonMapping ?? true)
            clientWrapper = new UnderscoreToColonMappingInfisicalClientWrapper(clientWrapper);

        return new InfisicalConfigurationProvider(providerLogger, options, clientWrapper);
    }
}
