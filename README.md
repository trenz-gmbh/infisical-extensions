# InfisicalExtensions

This project adds extension methods for the [Infisical .NET SDK](https://infisical.com/docs/sdks/languages/csharp) to
register it as a configuration provider in your .NET app.

## Installation

```bash
dotnet add package TRENZ.Extensions.Infisical
```

## Usage

You will need four things to integrate this extension:

- Your site URL (`https://infisical.company.com`)
- A Client ID/secret pair
- A project ID

The site URL is obvious.
It's where you can access the web UI of your instance.

Client ID & secret are a bit more hidden:

1. From the home page of your instance, navigate to "Access Control"
2. Switch to the tab "Identities"
3. Choose an identity you want your app to use (or create one)
4. Under "Authentication", add or edit "Universal Auth"
5. Copy the Client ID
6. Click "Add Client Secret"
7. Give it a name
8. Copy the Client Secret

The last thing you need is the project ID.
You can find this here:

1. From the home page of your instance, navigate to "Projects"
2. Choose your project (or create one)
3. On the left sidebar, switch to "Project Settings" (**not** "Settings", that one is for the "Secrets Manager")
4. In the "General" tab -> "Project Overview", you will find a button "Copy Project ID"
5. Click it

After you gathered all this information, add them to your `appsettings.json`:

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

This will add an `InfisicalConfigurationProvider` that provides all available secrets through `IConfiguration`.

> [!Note]
> The provider drops all keys in the "Infisical" object in order to protect your infisical credentials.

## Managing an Infisical Project

When you manage an Infisical project you want to access with this extension, you need to mindful of the following
things:

### Environment does not equal Environment _Slug_

You can define different secret values for each environment your app runs in.
This is really helpful for scenarios where you have a production environment and a development environment.

This extension supports different environments and even picks up the current `IHostEnvironment.EnvironmentName` and
includes it when asking for the secrets.

However, when managing your Infisical project environments, notice how your environment has a name, and a slug (as seen
here in the Secrets Manager settings):

!["Environments" section in the Infisical Secrets Manager settings](https://raw.githubusercontent.com/trenz-gmbh/infisical-extensions/main/docs/environments-and-slugs.png)

This extension uses the environment names used by .NET, for example `Development`, but converted to lowercase
(`development`).
This must match one of your environment _slugs_.

When adding or changing an environment, you need to be mindful of the impact this has for your infisical project.

### Enforced Capitalization

Infisical likes to enforce capitalized secret key names.
This is usually not what you want for your `IConfiguration` keys.

You can disable automatic capitalization by turning off this option in the Secrets Manager settings:

!["Enforce Capitalization" option in the Infisical Secrets Manager settings](https://raw.githubusercontent.com/trenz-gmbh/infisical-extensions/main/docs/enforce-capitalization-option.png)

### Automatic secret key mapping

Since Infisical doesn't allow colons (`:`) in the secret key anymore, we have integrated a mechanism that allows you
to use double underscores (`__`) instead.
Any key with double underscores will appear with colons in your `IConfiguration`.

For example:

- You added a secret with the key `ConnectionStrings__DB`
- You use this extension to load it into your `IConfiguration`
- You can access the secret using `IConfiguration.GetConnectionString("DB")`
  - this works, because then `GetConnectionString` extension method expects a key `ConnectionStrings:<name>`, and
  - this extension translates `ConnectionStrings__DB` to `ConnectionStrings:DB`

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

## License

Licensed under MIT. For more information, see [LICENSE](LICENSE)
