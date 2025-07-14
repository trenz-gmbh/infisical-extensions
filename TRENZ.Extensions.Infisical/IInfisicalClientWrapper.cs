using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public interface IInfisicalClientWrapper
{
    IDictionary<string, SecretElement>? GetAll();
}
