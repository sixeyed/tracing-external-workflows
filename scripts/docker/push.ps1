pushd "${PSScriptRoot}/../../docker"

try {
    $info = docker version -f json | ConvertFrom-Json
    $env:DOCKER_BUILD_OS = $info.Server.Os.ToLower()
    $env:DOCKER_BUILD_CPU = $info.Server.Arch.ToLower()

    docker compose `
        -f ./docker-compose.yml `
        -f ./docker-compose-build.yml `
        -f ./docker-compose-build-tags.yml `
        push
}

finally {
    popd
}