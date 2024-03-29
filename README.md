# Money Monitor

![Tray screenshot](https://github.com/stevehjohn/MoneyMonitor/blob/master/assets/screenshot.png)

## Overview

This is an app to keep you abreast of your cryptocurrency investments. It shows in your system tray whether your investments have gone up or down. It can also display charts of your investments over time.

## Installation

I won't provide binaries for this as given the sensitive nature of API keys, I think source code transparency is important, so you'll need to build it yourself. [VS 2019 Community](https://visualstudio.microsoft.com/vs/community/) will do just fine, or `dotnet build` from the [SDK](https://dotnet.microsoft.com/download) if you're a command line warrior.

## Configuration

In `appSettings.json` set `PollInterval` to a value of your liking.

If you would like to monitor multiple exchanges, configure them as below and comma separate the list of exchanges in the `Clients` setting, e.g. `BinanceExchangeClient,CoinbaseExchangeClient`.

### Binance

In Binance, create an API key. The default permissions (`Enable Reading`) should suffice.

Set `BinanceCredentials.ApiKey` and `BinanceCredentials.SecretKey` in `appSettings.json` with the values obtained from Binance. Add `BinanceExchangeClient` to the `Clients` setting.

### Coinbase

In Coinbase, create an API key with the permission `wallet:accounts:read`.

Set `CoinbaseCredentials.ApiKey` and `CoinbaseCredentials.ApiSecret` in `appSettings.json` with the values obtained from Coinbase. Add `CoinbaseExchangeClient` to the `Clients` setting.

### Coinbase Pro

In Coinbase Pro, create an API key with `View` permissions.

Set `CoinbaseProCredentials.ApiKey`, `CoinbaseProCredentials.ApiSecret` and `CoinbaseProCredentials.Passphrase` in `appSettings.json` with the values obtained from Coinbase Pro. Add `CoinbaseProExchangeClient` to the `Clients` setting.

## Exchange Rates

Depending on the exchange, its API may not return exchange rate details for a particular crypto currency to your preferred fiat currency. In this instance, the monitor can be configured to request your balance in an intermediate currency, then automatically convert it to your preferred fiat.

So, for example, if Coinbase Pro only supports XLM-EUR and your preferred fiat is GBP you can configure the monitor to perform this conversion with the following in `appSettings.json`.

``` json
"ExchangeRateFallbacks": [
  {
    "Exchange": "CoinbasePro",
    "CryptoCurrency": "XLM",
    "FiatCurrency": "EUR"
  }
],
"FiatCurrency": "GBP"
```

## User Interface

The tray icon will display an arrow pointing up, down or right depending whether your investment has gone up, down or stayed the same respectively.

Hover over the icon for a quick summary which will show the time the balances were last checked, the all time high, the current value and the all time low.

Click on the icon for a chart showing the history of all investments. This form will close when you click elsewhere on the screen.

Right click the icon for the menu. Exit is self explanatory. Click on a currency to show a history window for that currency. You may also choose whether to keep these windows on top of all others from this menu.

### Excel Spreadsheet Integration

The app can also optionally update a cell in a spreadsheet with your current balance (if you're nerd like me who tracks finances in a spreadsheet). Simply put the path to the spreadsheet in the `ExcelFilePath` app setting and put the cell in `ExcelCell`.

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
``` json
{
  "CoinbaseApiKey": "my-api-key",
  "CoinbaseApiSecret": "my-api-secret"
}
```

`\src\MoneyMonitor.Windows\appSettings.json`
``` json
{
  "CoinbaseCredentials": {
    "ApiKey": "{CoinbaseApiKey}",
    "ApiSecret": "{CoinbaseApiSecret}"
  }
}
```

# Trading

Trading is performed by a separate application, `MoneyMonitor.Trader.Console`. This is so that it can be run independently of the Windows monitoring UI.
The trading application currently only supports Coinbase Pro.
It could be run on an AWS instance for example, potentially in the same region as the Coinbase Pro API for speedier communication.

## Configuration

Secrets held within `consoleSettings.json` can be updated in the same way as the Windows application. See [Secrets](#secrets) above.

You will need to also add the `Trade` permission to the API key you create.