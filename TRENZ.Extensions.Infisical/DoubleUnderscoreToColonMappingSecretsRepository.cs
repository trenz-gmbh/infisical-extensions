using System.Collections.Frozen;
using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public class DoubleUnderscoreToColonMappingSecretsRepository(ISecretsRepository inner) : ISecretsRepository
{
    public IDictionary<string, Secret>? GetAllSecrets()
    {
        var allSecrets = inner.GetAllSecrets();
        if (allSecrets == null)
            return null;

        var newEntries = new Dictionary<string, Secret>();
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
