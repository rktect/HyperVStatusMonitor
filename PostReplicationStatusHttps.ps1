$json = Measure-VMReplication | ConvertTo-Json -Compress

# the following is to allow for using self-signed certificates - leave out otherwise
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy
# end self-signed certificates code

$result = Invoke-WebRequest -Uri "https://{your-azure-sitename}.azurewebsites.net/api/status" -Method POST -Body $json -ContentType application/json -UseBasicParsing
$result