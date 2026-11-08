param([switch]$XuatCauTruc)
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    if ([string]::IsNullOrWhiteSpace($env:QLMB_SQL_CONNECTION)) {
        $env:QLMB_SQL_CONNECTION = 'Server=localhost;Database=BanVeMayBay;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;Connect Timeout=10'
    }
    $env:QLMB_SQL_MAPPING_FILE = Join-Path $PWD 'docs/sql-mapping.BanVeMayBay.json'
    if ($XuatCauTruc) {
        $schemaPath = Join-Path $PWD 'BanVeMayBay.schema.local.json'
        dotnet run --project tools/SqlInspector/SqlInspector.csproj -- --schema $schemaPath
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        Write-Host "File cau truc: $schemaPath"
    }
    else {
        dotnet run --project QuanLyMayBay.csproj
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}
finally { Pop-Location }
