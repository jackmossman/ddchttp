# ddchttp
http wrapper for ddcutil

## Features

- **Native AOT Compilation**: Fast startup and low memory footprint
- **Rate Limiting**: Serialized access to hardware (one request at a time)
- **Flexible Input Mapping**: Configure IP-based swap targets for multi-machine setups
- **Systemd Integration**: Run as a background service on Linux

## Requirements

- Linux with `ddcutil` installed (`apt install ddcutil`)
- .NET 10 SDK (for building)
- I2C permissions configured for the service user

## Quick Start

### 1. Clone and Configure

```bash
git clone https://github.com/your-username/ddchttp.git
cd ddchttp
cp src/ddchttp.web/settings.example.json src/ddchttp.web/settings.json
```

Edit `settings.json` to configure your inputs and IP mappings.

### 2. Build

```bash
dotnet build
```

### 3. Run (Development)

```bash
dotnet run --project src/ddchttp.web
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Health check |
| `POST` | `/input/{name}` | Set input by name (e.g., `HDMI1`, `DP`, `USBC`) |
| `POST` | `/input/swap/{type}` | Swap to next input matching type, excluding caller's IP |

## Configuration

See [settings.example.json](src/ddchttp.web/settings.example.json) for configuration options:

- `Site.Port`: HTTP port (default: 42024)
- `Controllers.Input.AvailableInputs`: Array of input definitions with IP, name, and swap groups

## Deployment

Use the provided PowerShell script to build and deploy to a remote Linux host:

```powershell
./scripts/build_deploy.ps1 -RemoteHost "your-server" -RemoteUser "your-user"
```

This will:
1. Build a native AOT binary using Docker
2. Copy the binary and configuration to the remote host
3. Install and start the systemd service

## License

See [LICENSE](LICENSE) for details.
