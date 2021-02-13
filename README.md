# Money Monitor

## Usage

## Installation

I won't provide binaries for this as given the sensitive nature of API keys, I think source code transparency is important, so you'll need to build it yourself. [VS 2019 Community](https://visualstudio.microsoft.com/vs/community/) will do just fine, or `dotnet build` from the [SDK](https://dotnet.microsoft.com/download) if you're a command line warrior.

## Configuration

In `appSettings.json` set `PollInterval` to a value of your liking.

### Coinbase

In Coinbase, create an API key with the permission `wallet:accounts:read`.

Set `CoinbaseCredentials.ApiKey` and `CoinbaseCredentials.ApiSecret` in `appSettings.json` with the values obtained from Coinbase.

## User Interface

The tray icon will display an arrow pointing up, down or right depending whether your investment has gone up, down or stayed the same respectively.

Hover over the icon for a quick summary which will show the time the balances were last checked, the all time high, the current value and the all time low.

Click on the icon for a chart showing the history of all investments. This form will close when you click elsewhere on the screen.

Right click the icon for the menu. Exit is self explanatory. Click on a currency to show a history window for that currency. You may also choose whether to keep this windows on top of all others from this menu.

## Auto Start

I've configured mine to run on system start.

I created a folder off of the root of `C:\` called `dotnet Apps`. Within there, I created a subfolder `Coinbase.BalanceMonitor`. In there, I put the output of a `Release` build, and set the `appSettings.json` accordingly.

Then, pressed `Win + R` and typed `shell:startup` to open an explorer view of startup shortcuts. Finally, dragged a shortcut to the exe into that location. When dragging the exe, hold `ctrl` and `shift` to create a shortcut.

## Secrets

If you plan on developing this application further, you'll want your API keys deployed to the build folder, but not set in the original `appSettings.json` (as they could end up in source control).

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