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
