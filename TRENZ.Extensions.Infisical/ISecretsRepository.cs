using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public interface ISecretsRepository
{
    IDictionary<string, SecretElement>? GetAllSecrets();
}
