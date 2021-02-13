# Money Monitor

## Secrets

There is a post-build step which will copy values from `\secrets\secrets.json` into `appSettings.json` in the output folder.
This is to prevent accidental check-in of secrets (API keys, passwords etc...) to source control as the `secrets` folder is excluded by `.gitignore`.
Any value surrounded by braces `{}` in `appSettings.json` will be replaced if there is a corresponding entry in `secrets.json`.

`\secrets\secrets.json`
```
{
  "CoinbaseApiKey": "my-api-key",
  "CoinbaseApiSecret": "my-api-secret"
}
```

`\src\MoneyMonitor.Windows\appSettings.json`
```
{
  "CoinbaseCredentials": {
    "ApiKey": "{CoinbaseApiKey}",
    "ApiSecret": "{CoinbaseApiSecret}"
  }
}
```