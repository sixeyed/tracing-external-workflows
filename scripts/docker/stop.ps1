
pushd "${PSScriptRoot}/../../docker"

try {
    docker compose `
        -f docker-compose.yml `
        -f docker-compose-monitoring.yml `
        down
}

finally {
    popd
}