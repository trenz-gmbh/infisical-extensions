# InfisicalExtensions

This project adds extension methods for the [Infisical .NET SDK](https://infisical.com/docs/sdks/languages/csharp) to
register it as a configuration provider in your .NET app.

## Usage

Disable End-to-End encryption in your Infisical project settings. The .NET SDK does not support it yet:

![Screenshot of disabled End-to-End Encryption checkbox](https://raw.githubusercontent.com/trenz-gmbh/infisical-extensions/main/docs/disable-e2ee.png)

> **Note**
> While you're in the settings, we also recommend to disable "Auto Capitalization"

Add your infisical settings to `appsettings.json`:

```json
{
  "Infisical": {
    "SiteUrl": "https://<your infisical host>",
    "ClientId": "07ebc18f-df32-475a-8fef-1bdd79a5c7ac",
    "ClientSecret": "insert-your-client-secret",
    "ProjectId": "some-project-id"
  }
}
```

Call `AddInfisicalConfiguration` on your application builder:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddInfisicalConfiguration();

// ...
```

This will add a `InfisicalConfigurationProvider` that provides all available secrets through `IConfiguration`.

> **Note**
> The provider drops all keys in the "Infisical" object to protect your infisical credentials.

---

Suppose you want to store your connection string in Infisical.

You first need to add a secret in the respective environment through the infisical interface:

> Note that infisical doesn't support nested secrets. The keys of the secrets need to include ":" to represent nested
> keys in an `appsettings.json`.

![Screenshot of Infisical with a secret called "ConnectionStrings:MyDatabase"](https://raw.githubusercontent.com/trenz-gmbh/infisical-extensions/main/docs/example-screenshot.png)

Then, in code, you can inject `IConfiguration` in your class and access its value as if it was in your
`appsettings.json`:

```csharp
public class MyConnectionStringProvider(IConfiguration configuration) {
    public string GetConnectionString() {
        // The following call looks up the key "ConnectionStrings:MyDatabase" in IConfiguration
        return configuration.GetConnectionString("MyDatabase");
    }
}
```

You could also access the key directly using:

```csharp
var connectionString = configuration["ConnectionStrings:MyDatabase"];
```

## Polling for changes

You can let this provider regularly poll Infisical for changes.
This is useful if you want to update your secrets without restarting your application.

To enable polling, you can set the `Infisical:PollingInterval` key in your `appsettings.json`:

```json
{
  "Infisical": {
    ...
    "PollingInterval": 10000
  }
}
```

This value is the interval in milliseconds in which the provider will poll Infisical for changes.
To disable polling, just remove the key from your configuration.

The default is to not poll for changes.

## Load timeout

You can set the `Infisical:LoadTimeout` key in your `appsettings.json` to specify the maximum time the provider will
wait for the initial (and subsequent) loads of the secrets.

```json
{
  "Infisical": {
    ...
    "LoadTimeout": 10000
  }
}
```

The default timeout is `5000` (5s).
To disable the timeout, set the value to `-1`.

## Installation

```bash
dotnet add package TRENZ.Extensions.Infisical
```

## License

Licensed under MIT. For more information, see [LICENSE](LICENSE)
