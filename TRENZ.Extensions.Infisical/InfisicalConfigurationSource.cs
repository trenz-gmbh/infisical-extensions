using Microsoft.Extensions.Configuration;

namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationSource(InfisicalConfigurationOptions options) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new InfisicalConfigurationProvider(options);
    }
}
