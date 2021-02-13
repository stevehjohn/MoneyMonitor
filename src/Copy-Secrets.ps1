$secrets = Get-Content ..\..\secrets\secrets.json | ConvertFrom-Json

$settings = Get-Content $args[0]

$secrets.PSObject.Properties | ForEach-Object {
    $settings = $settings -replace "{$($_.Name)}", $_.Value
}

Set-Content -Path $args[0] -Value $settings
