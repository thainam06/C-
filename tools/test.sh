#!/usr/bin/env bash
set -euo pipefail
cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.."
dotnet build QuanLyMayBay.csproj -c Release
for project in Tests/KhachHangChecks/Checks.csproj Tests/HanhKhachChecks/Checks.csproj Tests/NhanVienKhuyenMaiChecks/Checks.csproj Tests/DoAnChecks/Checks.csproj; do
  dotnet run --project "$project" -c Release
done
