using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public interface ISecretsRepository
{
    Task<IDictionary<string, SecretElement>?> GetAllSecretsAsync(CancellationToken cancellationToken = default);
}
