$json = Measure-VMReplication | ConvertTo-Json -Compress
$result = Invoke-WebRequest -Uri "http://{your-azure-sitename}.azurewebsites.net/api/status" -Method POST -Body $json -ContentType application/json -UseBasicParsing
$result