using System.Collections.Frozen;
using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public class UnderscoreToColonMappingInfisicalClientWrapper(IInfisicalClientWrapper inner) : IInfisicalClientWrapper
{
    public IDictionary<string, SecretElement>? GetAll()
    {
        var all = inner.GetAll();
        if (all == null)
            return null;

        var newEntries = new Dictionary<string, SecretElement>();
        foreach (var k in all.Keys)
        {
            if (!k.Contains("__"))
                continue;

            var mappedKey = k.Replace("__", ":");

            // clone secret with new key
            newEntries[mappedKey] = all[k];
        }

        return FrozenDictionary.ToFrozenDictionary([..all, ..newEntries]);
    }
}
