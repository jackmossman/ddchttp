param(
    [Parameter(Mandatory=$true)][string]$RemoteHost,
    [Parameter(Mandatory=$true)][string]$RemoteUser,
    [int]$RemoteSSHPort = 22,
    [string]$RemotePath = "/tmp",
    [string]$Subnet = "192.168.1.0/24",
    [string]$ServiceFile = "ddchttp.service",
    [string]$SettingsFile = "src/ddchttp.web/settings.json",
    [string]$Runtime = "linux-arm64",
    [switch]$SkipBuild
)

$cwd = (Get-Location).Path
Write-Host "Working directory: $cwd"

$targetBinary = Join-Path $cwd "ddchttp"

if (-not $SkipBuild) {
    # Build using Docker (.NET 10 SDK with AOT)
    Write-Host "Building for runtime: $Runtime using Docker..."

    $sdkImage = 'mcr.microsoft.com/dotnet/sdk:10.0'

    $publishDir = Join-Path $cwd "publish"
    if (Test-Path $publishDir) { Remove-Item -Path $publishDir -Recurse -Force }

    # For cross-compilation to ARM64, install the cross-toolchain in x64 container
    if ($Runtime -like 'linux-arm64') {
        $buildCmd = "docker run --rm --platform linux/amd64 -v ""${cwd}:/src"" -w /src $sdkImage bash -c ""apt-get update && apt-get install -y clang llvm gcc-aarch64-linux-gnu && dotnet publish src/ddchttp.web/ddchttp.web.csproj -c Release -r $Runtime -p:PublishAot=true -p:PublishTrimmed=true --self-contained true -o /src/publish"""
    } else {
        $buildCmd = "docker run --rm -v ""${cwd}:/src"" -w /src $sdkImage bash -c ""dotnet publish src/ddchttp.web/ddchttp.web.csproj -c Release -r $Runtime -p:PublishAot=true -p:PublishTrimmed=true --self-contained true -o /src/publish"""
    }
    Write-Host $buildCmd
    Invoke-Expression $buildCmd

    # Locate produced binary (native AOT will typically create an executable named 'ddchttp.web' in publish)
    $publishedBinary = Join-Path $publishDir "ddchttp.web"

    if (Test-Path $publishedBinary) {
        Move-Item -Path $publishedBinary -Destination $targetBinary -Force
        Remove-Item -Path $publishDir -Recurse -Force
    }

    if (-not (Test-Path $targetBinary)) {
        Write-Error "Build failed: ddchttp binary not found in $publishDir"
        exit 1
    }
} else {
    Write-Host "Skipping build..."
    if (-not (Test-Path $targetBinary)) {
        Write-Error "Binary not found: $targetBinary. Run without -SkipBuild first."
        exit 1
    }
}

# Ensure executable bit locally (for POSIX remote targets)
try {
    icacls $targetBinary | Out-Null
} catch {
    # Running on non-Windows environment inside PowerShell: use chmod via bash if available
    if (Get-Command chmod -ErrorAction SilentlyContinue) {
        & chmod 0755 $targetBinary
    }
}

# Use static remote deploy script from repo (scripts/deploy_remote.sh)
# Copy binary, service ini, settings, and remote script to remote
# Use POSIX-style paths when copying to remote host; ensure paths are quoted
Write-Host "Copying binary, service file, and settings to remote host..."
$localBinaryPath = Join-Path $cwd "ddchttp"
$localServicePath = Join-Path $cwd $ServiceFile
$localSettingsPath = Join-Path $cwd $SettingsFile
$localDeployPath = Join-Path $cwd "scripts/deploy_remote.sh"

# Batch all files in a single scp command
$scpCmd = "scp -P $RemoteSSHPort '$localBinaryPath' '$localServicePath' '$localSettingsPath' '$localDeployPath' $RemoteUser@${RemoteHost}:$RemotePath/"
Write-Host $scpCmd
Invoke-Expression $scpCmd

# Run remote deploy script with args: <REMOTE_PATH> <SERVICE_FILE> <SUBNET>
$sshRunCmd = "ssh -t -p $RemoteSSHPort $RemoteUser@${RemoteHost} ""bash $RemotePath/deploy_remote.sh '$RemotePath' '$ServiceFile' '$Subnet'; rm -f $RemotePath/deploy_remote.sh"""
Write-Host $sshRunCmd
Invoke-Expression $sshRunCmd

Write-Host "Deployment complete. Check the service status on the remote host with: sudo systemctl status ddchttp";