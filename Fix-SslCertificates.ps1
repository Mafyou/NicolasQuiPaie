# Fix-SslCertificates.ps1
# Script to fix SSL certificate issues for .NET development

Write-Host "?? Fixing SSL Certificate Issues for Nicolas Qui Paie..." -ForegroundColor Cyan

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if (-not $isAdmin) {
    Write-Host "??  This script should be run as Administrator for best results." -ForegroundColor Yellow
    Write-Host "   Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "1?? Checking current certificate status..." -ForegroundColor Green
dotnet dev-certs https --check --trust

Write-Host ""
Write-Host "2?? Cleaning old certificates..." -ForegroundColor Green
dotnet dev-certs https --clean

Write-Host ""
Write-Host "3?? Creating new development certificate..." -ForegroundColor Green
dotnet dev-certs https --trust

Write-Host ""
Write-Host "4?? Verifying certificate installation..." -ForegroundColor Green
dotnet dev-certs https --check --trust

Write-Host ""
Write-Host "?? Browser-specific solutions:" -ForegroundColor Cyan
Write-Host "   • Chrome: Type 'thisisunsafe' on the warning page" -ForegroundColor Gray
Write-Host "   • Edge: Click 'Advanced' ? 'Continue to localhost'" -ForegroundColor Gray
Write-Host "   • Firefox: Click 'Advanced' ? 'Accept the Risk and Continue'" -ForegroundColor Gray

Write-Host ""
Write-Host "?? Project URLs:" -ForegroundColor Cyan
Write-Host "   • NicolasQuiPaieWeb: https://localhost:7084" -ForegroundColor Gray
Write-Host "   • NicolasQuiPaieAPI: https://localhost:7051" -ForegroundColor Gray

Write-Host ""
Write-Host "? Certificate setup complete!" -ForegroundColor Green
Write-Host "   Restart your browser and try accessing the applications again." -ForegroundColor White

Read-Host "Press Enter to continue..."