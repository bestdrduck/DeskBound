$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$source = Join-Path $root "src\DeskBound.cs"
$icon = Join-Path $root "assets\DeskBound.ico"
$logo = Join-Path $root "assets\DeskBound-logo.png"
$sidebarManage = Join-Path $root "assets\emoji\sidebar-manage.png"
$sidebarAppearance = Join-Path $root "assets\emoji\sidebar-appearance.png"
$sidebarHelp = Join-Path $root "assets\emoji\sidebar-help.png"
$emojiLicense = Join-Path $root "assets\emoji\LICENSE.txt"
$outputDir = Join-Path $root "outputs"
$output = Join-Path $outputDir "桌伴.exe"

$framework = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319"

if (-not (Test-Path (Join-Path $framework "csc.exe"))) {
    $framework = Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319"
}

$compiler = Join-Path $framework "csc.exe"
$wpf = Join-Path $framework "WPF"

if (-not (Test-Path $compiler)) {
    throw "C# compiler not found: $compiler"
}

if (-not (Test-Path $source)) {
    throw "Source file not found: $source"
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$argsList = @(
    "/nologo"
    "/target:winexe"
    "/platform:anycpu"
    "/optimize+"
    "/out:$output"
    "/win32icon:$icon"
    "/resource:$logo,DeskBound.logo.png"
    "/resource:$sidebarManage,DeskBound.sidebar-manage.png"
    "/resource:$sidebarAppearance,DeskBound.sidebar-appearance.png"
    "/resource:$sidebarHelp,DeskBound.sidebar-help.png"
    "/resource:$emojiLicense,DeskBound.fluent-emoji-license.txt"
    "/reference:$(Join-Path $wpf 'PresentationCore.dll')"
    "/reference:$(Join-Path $wpf 'PresentationFramework.dll')"
    "/reference:$(Join-Path $wpf 'WindowsBase.dll')"
    "/reference:$(Join-Path $framework 'System.Xaml.dll')"
    "/reference:$(Join-Path $framework 'System.dll')"
    "/reference:$(Join-Path $framework 'System.Core.dll')"
    "/reference:$(Join-Path $framework 'System.Drawing.dll')"
    "/reference:$(Join-Path $framework 'System.Windows.Forms.dll')"
    "/reference:$(Join-Path $framework 'System.Web.Extensions.dll')"
    $source
)

Write-Host "Building 桌伴..."

& $compiler @argsList

if ($LASTEXITCODE -ne 0) {
    throw "Build failed. Exit code: $LASTEXITCODE"
}

Write-Host ""
Write-Host "Build successful."
Write-Host $output

Get-Item $output
