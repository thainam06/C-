$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
    dotnet build QuanLyMayBay.csproj -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    foreach ($projectPath in @(
        'Tests/KhachHangChecks/Checks.csproj',
        'Tests/HanhKhachChecks/Checks.csproj',
        'Tests/NhanVienKhuyenMaiChecks/Checks.csproj',
        'Tests/DoAnChecks/Checks.csproj'
    )) {
        dotnet run --project $projectPath -c Release
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    Write-Host 'Build va kiem tra Core hoan tat. Van can kiem tra giao dien Windows va SQL Server thuc te.'
}
finally { Pop-Location }
