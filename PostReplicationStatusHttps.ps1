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

$endPoint = "http://{your-azure-sitename}.azurewebsites.net/api/status"
$eventSource = "Hyper-V Replication Monitor"

try
{
    $json = Measure-VMReplication | ConvertTo-Json -Compress
    
    Write-Host ("Updating status at "+$endPoint)
    $wr = [System.Net.HttpWebRequest]::Create($endPoint)
    $wr.Method= 'POST';
    $wr.ContentType="application/json";
    $utf8Bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $wr.Timeout = 10000;

    $Stream = $wr.GetRequestStream();
    $Stream.Write($utf8Bytes, 0, $utf8Bytes.Length);
    $Stream.Flush();
    $Stream.Close();

    $resp = $wr.GetResponse().GetResponseStream()
    $sr = New-Object System.IO.StreamReader($resp) 
    $respTxt = $sr.ReadToEnd()

    Write-Host "$($respTxt)";
}
catch
{
    $_.Exception
    Write-EventLog –LogName Application –Source $eventSource –EntryType Error –EventID 1 –Message $_.Exception
}