using System.Collections.Frozen;
using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public class UnderscoreToColonMappingInfisicalClientWrapper(IInfisicalClientWrapper inner) : IInfisicalClientWrapper
{
    public IDictionary<string, SecretElement>? GetAll()
    {
        var allSecrets = inner.GetAll();
        if (allSecrets == null)
            return null;

        var newEntries = new Dictionary<string, SecretElement>();
        foreach (var key in allSecrets.Keys)
        {
            if (!key.Contains("__"))
                continue;

            var mappedKey = key.Replace("__", ":");

            // clone secret with new key
            newEntries[mappedKey] = allSecrets[key];
        }

        return FrozenDictionary.ToFrozenDictionary([..allSecrets, ..newEntries]);
    }
}
