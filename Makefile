BINARY := ddchttp
RUNTIME ?= linux-arm64
PUBLISH_DIR := ./publish
SERVICE := ddchttp.service
SETTINGS := src/ddchttp.web/settings.json
SUBNET ?= 192.168.1.0/24

.PHONY: build publish install deploy deploy-firewall remote-deploy clean

build:
	dotnet publish -c Release -r $(RUNTIME) -o $(PUBLISH_DIR)

# Native AOT publish using Docker (matches build_deploy.ps1 behavior)
publish:
	@if [ "$(RUNTIME)" = "linux-arm64" ]; then \
		docker run --rm --platform linux/amd64 -v "$(PWD):/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 bash -c "apt-get update && apt-get install -y clang llvm gcc-aarch64-linux-gnu && dotnet publish src/ddchttp.web/ddchttp.web.csproj -c Release -r $(RUNTIME) -p:PublishAot=true -p:PublishTrimmed=true --self-contained true -o /src/$(PUBLISH_DIR)"; \
	else \
		docker run --rm -v "$(PWD):/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 bash -c "dotnet publish src/ddchttp.web/ddchttp.web.csproj -c Release -r $(RUNTIME) -p:PublishAot=true -p:PublishTrimmed=true --self-contained true -o /src/$(PUBLISH_DIR)"; \
	fi

install:
	sudo cp $(PUBLISH_DIR)/$(BINARY) /usr/local/bin/$(BINARY)
	sudo chown root:root /usr/local/bin/$(BINARY) || true
	sudo chmod 0755 /usr/local/bin/$(BINARY)
	sudo cp $(SETTINGS) /usr/local/bin/settings.json
	# ensure service user exists (non-fatal if already present)
	sudo id -u ddchttp >/dev/null 2>&1 || sudo useradd --system --no-create-home --shell /usr/sbin/nologin ddchttp || true
	sudo chown ddchttp:ddchttp /usr/local/bin/settings.json || true
	sudo cp deploy/$(SERVICE) /etc/systemd/system/$(SERVICE)
	sudo systemctl daemon-reload
	sudo systemctl enable $(BINARY)
	sudo systemctl restart $(BINARY)

deploy: publish install deploy-firewall

deploy-firewall:
	sudo ufw allow from $(SUBNET) to any port 42024 proto tcp

# Call the PowerShell-based remote deploy script (user supplies REMOTE_* variables)
remote-deploy:
	./scripts/build_deploy.ps1 -RemoteHost "$(REMOTE_HOST)" -RemoteUser "$(REMOTE_USER)" -RemoteSSHPort "$(REMOTE_SSH_PORT)" -RemotePath "$(REMOTE_PATH)" -Runtime "$(RUNTIME)" -ServiceFile "deploy/$(SERVICE)" -SettingsFile "$(SETTINGS)"

clean:
	rm -rf $(PUBLISH_DIR) $(BINARY)

