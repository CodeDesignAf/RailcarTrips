param(
    [string]$DatabasePath = "$PSScriptRoot\src\RailcarTrips.Api\railcartrips.db",
    [string]$OutputCsvPath = "$env:USERPROFILE\Downloads\equipment-events-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
)

$sqlite = Get-Command sqlite3 -ErrorAction SilentlyContinue
if (-not $sqlite) {
    throw "sqlite3 is not installed. Install it first (example: winget install SQLite.SQLite)."
}

if (-not (Test-Path -LiteralPath $DatabasePath)) {
    throw "Database file not found: $DatabasePath"
}

$query = @"
SELECT
    Id,
    EquipmentId,
    CityId,
    Code,
    EventUtcTime,
    EventLocalTime,
    NaturalKeyHash
FROM EquipmentEvents
ORDER BY EventUtcTime, Id;
"@

$csv = & sqlite3 $DatabasePath -header -csv $query
if ($LASTEXITCODE -ne 0) {
    throw "sqlite3 query failed. Verify DB path and table name."
}

$outputDirectory = Split-Path -Path $OutputCsvPath -Parent
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -Path $outputDirectory -ItemType Directory -Force | Out-Null
}

$csv | Set-Content -Path $OutputCsvPath -Encoding UTF8

Write-Host "Export completed:"
Write-Host $OutputCsvPath
