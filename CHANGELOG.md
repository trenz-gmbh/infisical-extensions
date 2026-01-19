# Unreleased

- Upgrade the Infisical SDK package to 3.0.4

# 1.1.2

- Bump release

# 1.1.1

- Fix issue with disposing the provider (NotImplementedException when clearing secrets)
- Added lots of trace logging to help debugging
- Updated to .NET 9

# 1.1.0

- Updated dependencies
- Always handle errors gracefully (removed `PropagateExceptions`)
- Added `EnableUnderscoreToColonMapping` option, which maps keys like `ConnectionStrings__DB` to
  `ConnectionStrings:DB` (both keys are available, and yield the same secret); fixes #7
- Silently removes trailing slashes in `SiteUrl` and throws if it doesn't use the HTTPS scheme; fixes #6

# 1.0.6

- You can now set a `PropagateExceptions` boolean to specify whether errors while loading the secrets should be
  propagated or suppressed
- Added exponential backoff for failed requests (max 10 failed requests; the `LoadTimeout` has precedence)
- Updated to the latest Infisical SDK

# 1.0.5

- Package now includes source link to GitHub sources

# 1.0.4

- Added `LoadTimeout` to specify the timeout for loading secrets from Infisical

# 1.0.3

- Fixed issue where no keys from `appsettings.json` where available after adding infisical
    - Now, all keys except `Infisical:*` are available

# 1.0.2

- Better handling for nested child keys
- Added support for `PollingInterval` to detect changes in secrets

# 1.0.1

- Fixed an issue where an invalid `AccessToken` was set on the Infisical client settings

# 1.0.0

- Initial release
- Support for adding Infisical to `IConfiguration` via `appsettings.json`
