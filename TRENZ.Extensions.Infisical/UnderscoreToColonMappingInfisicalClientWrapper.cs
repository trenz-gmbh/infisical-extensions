using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical;

public class UnderscoreToColonMappingInfisicalClientWrapper(IInfisicalClientWrapper inner) : IInfisicalClientWrapper
{
    public IDictionary<string, SecretElement>? GetAll()
    {
        var all = inner.GetAll();
        if (all == null)
            return null;

        foreach (var k in all.Keys)
        {
            if (!k.Contains("__"))
                continue;

            var mappedKey = k.Replace("__", ":");

            // clone secret with new key
            all[mappedKey] = all[k];
        }

        return all;
    }
}
