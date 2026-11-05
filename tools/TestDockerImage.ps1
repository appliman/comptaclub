#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Image,

    [ValidateSet('Production', 'development')]
    [string]$Environment = 'Production'
)

$ErrorActionPreference = 'Stop'
$containerName = "comptaclub-assets-check-$([Guid]::NewGuid().ToString('N'))"

try
{
    # No production volume or credentials: SQLite lives in this disposable container.
    docker run --detach --name $containerName --publish '127.0.0.1::8080' `
        --env "ASPNETCORE_ENVIRONMENT=$Environment" `
        --env 'ASPNETCORE_URLS=http://+:8080' `
        --env 'ComptaClub__DatabaseProvider=Sqlite' `
        --env 'ComptaClub__ConnectionString=Data Source=/app/data/assets-check.db' `
        --env 'ComptaClub__AdminUserEmail=docker-check@example.invalid' `
        --env 'ComptaClub__TempFolder=/app/data/temp' `
        --env 'ComptaClub__OtlpEndpoint=' $Image | Out-Null
    if ($LASTEXITCODE -ne 0)
    {
        throw "Impossible de démarrer l'image $Image."
    }

    $portBinding = docker port $containerName '8080/tcp'
    if ($LASTEXITCODE -ne 0)
    {
        throw 'Impossible de déterminer le port du conteneur.'
    }
    $baseUri = "http://$($portBinding.Trim())"
    $deadline = [DateTime]::UtcNow.AddSeconds(60)
    $page = $null
    while ([DateTime]::UtcNow -lt $deadline)
    {
        try
        {
            $page = Invoke-WebRequest "$baseUri/" -TimeoutSec 3 -MaximumRedirection 0
            break
        }
        catch
        {
            Start-Sleep -Milliseconds 500
        }
    }
    if ($null -eq $page -or $page.StatusCode -ne 200 -or $page.Content -notmatch '_framework/blazor\.web\.js')
    {
        throw 'La page ne répond pas HTTP 200 avec le script Blazor attendu.'
    }

    $script = Invoke-WebRequest "$baseUri/_framework/blazor.web.js" -TimeoutSec 10 -MaximumRedirection 0
    if ($script.StatusCode -ne 200 -or $script.Headers['Content-Type'] -notmatch 'javascript' `
        -or $script.RawContentLength -lt 10000 -or $script.Content -notmatch 'Blazor' `
        -or $script.Content -match '^\s*<!DOCTYPE')
    {
        throw 'Le bootstrap Blazor doit renvoyer HTTP 200 et un contenu JavaScript exploitable.'
    }

    $css = Invoke-WebRequest "$baseUri/css/site.css" -TimeoutSec 10 -MaximumRedirection 0
    if ($css.StatusCode -ne 200 -or $css.Headers['Content-Type'] -notmatch 'text/css')
    {
        throw 'La feuille de style doit répondre HTTP 200.'
    }

    $negotiate = Invoke-RestMethod "$baseUri/_blazor/negotiate?negotiateVersion=1" -Method Post -TimeoutSec 10
    if ([string]::IsNullOrWhiteSpace($negotiate.connectionToken) `
        -or 'WebSockets' -notin $negotiate.availableTransports.transport)
    {
        throw 'La négociation SignalR Blazor ne propose pas WebSockets.'
    }

    [pscustomobject]@{
        Image = $Image
        Environment = $Environment
        PageStatus = [int]$page.StatusCode
        BlazorScriptStatus = [int]$script.StatusCode
        BlazorScriptContentType = $script.Headers['Content-Type'] -join ''
        BlazorScriptBytes = $script.RawContentLength
        CssStatus = [int]$css.StatusCode
        BlazorWebSockets = $true
    }
}
catch
{
    docker logs --tail 30 $containerName 2>&1 | Out-Host
    throw
}
finally
{
    docker rm --force $containerName 2>&1 | Out-Null
}
