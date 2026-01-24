#!/bin/bash
set -e

# Usage: deploy_remote.sh <REMOTE_PATH> <SERVICE_FILE> <SUBNET>
REMOTE_PATH=${1:-/tmp}
SERVICE_FILE=${2:-ddchttp.service}
SUBNET=${3:-192.168.1.0/24}

# service user setup
echo "ensuring service user 'ddchttp' exists (non-fatal if already present)"
id -u ddchttp &>/dev/null || sudo useradd --system --no-create-home --shell /usr/sbin/nologin ddchttp

# i2c group and membership
echo "ensuring 'i2c' group exists and adding 'ddchttp' to it"
getent group i2c &>/dev/null || sudo groupadd i2c
sudo usermod -aG i2c ddchttp

# udev rules
echo "installing udev rules for i2c if missing"
if [ ! -f /etc/udev/rules.d/99-i2c.rules ]; then
    echo 'KERNEL=="i2c-[0-9]*", GROUP="i2c", MODE="0660"' | sudo tee /etc/udev/rules.d/99-i2c.rules
    sudo udevadm control --reload-rules
    sudo udevadm trigger
fi

# log directory
echo "creating /var/log/ddchttp and setting ownership"
sudo mkdir -p /var/log/ddchttp
sudo chown ddchttp:ddchttp /var/log/ddchttp

# binary
echo "installing binary to /usr/local/bin/ddchttp"
sudo mv "$REMOTE_PATH/ddchttp" /usr/local/bin/ddchttp || sudo cp "$REMOTE_PATH/ddchttp" /usr/local/bin/ddchttp
sudo chown root:root /usr/local/bin/ddchttp
sudo chmod 0755 /usr/local/bin/ddchttp

# settings file
echo "installing settings to /usr/local/bin/settings.json"
sudo cp "$REMOTE_PATH/settings.json" /usr/local/bin/settings.json
sudo chown ddchttp:ddchttp /usr/local/bin/settings.json
sudo chmod 0644 /usr/local/bin/settings.json

# service
echo "installing systemd service and restarting"
sudo cp "$REMOTE_PATH/$SERVICE_FILE" /etc/systemd/system/$SERVICE_FILE
sudo systemctl daemon-reload
sudo systemctl enable ddchttp
sudo systemctl restart ddchttp

# firewall
echo "ensuring UFW allows port 42024 from $SUBNET"
sudo ufw status | grep -q "42024" || sudo ufw allow from $SUBNET to any port 42024 proto tcp