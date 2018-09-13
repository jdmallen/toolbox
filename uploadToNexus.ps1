Param($drop, $endpoint, $creds)

$pkgs = gci -Recurse -Path $drop\**\*.nupkg
$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($creds))
$basicAuthValue = "Basic $encodedCreds"
$boundary = [System.Guid]::NewGuid().ToString()
$lf = "`r`n"
$headers = @{
    "Authorization" = $basicAuthValue;
    "Accept" = "application/json";
}

$pkgs | % {
    Write-Host "Uploading $($_.Name) to Nexus"
    $fileContent = [IO.File]::ReadAllBytes($_.FullName)
    $enc = [System.Text.Encoding]::GetEncoding("iso-8859-1")	
    $fileEnc = $enc.GetString($fileContent)
    $bodyLines = (
        "--$boundary",
        "Content-Disposition: form-data; name=`"nuget.asset`"; filename=`"$($_.Name)`"",
	    "Content-Type: application/octet-stream$lf",
        $fileEnc,
        "--$boundary--$lf"
    ) -join $lf

    Invoke-RestMethod -Method Post -Uri $endpoint -ContentType "multipart/form-data; boundary=`"$boundary`"" -Headers $headers -Body $bodyLines
}
