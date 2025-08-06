using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public interface IInfisicalClientWrapper
{
    Task<IDictionary<string, SecretElement>?> GetAllSecretsAsync(CancellationToken cancellationToken = default);
}
