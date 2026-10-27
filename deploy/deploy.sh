#!/usr/bin/env bash
# Deploy a version of the card game server.
# Usage (on the server, in /srv/cardgame): ./deploy.sh 0.2.3
set -euo pipefail

VERSION="${1:?Usage: ./deploy.sh VERSION (e.g. 0.2.3)}"
cd "$(dirname "$0")"

# Remember what was running, so we know what to roll back to
if [ -f .env ]; then
  cp .env .env.previous
fi
echo "CARDGAME_VERSION=$VERSION" > .env

docker compose pull
# --wait: return only when every service is healthy; fail if one isn't
docker compose up -d --wait --wait-timeout 120

# nginx.conf is a bind-mounted file, so Compose can't see changes to it.
# Check it, then reload gracefully (open player connections stay up).
docker compose exec -T proxy nginx -t
docker compose exec -T proxy nginx -s reload

docker compose ps

echo "Deployed cardgame-server $VERSION"