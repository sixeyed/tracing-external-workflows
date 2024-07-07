
pushd "${PSScriptRoot}/../../docker"

try {
    docker compose `
        -f docker-compose.yml `
        -f docker-compose-monitoring.yml `
        up -d

    echo "Open Grafana at http://localhost:3000"
}

finally {
    popd
}