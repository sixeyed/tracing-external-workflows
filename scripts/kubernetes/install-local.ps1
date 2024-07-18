pushd "${PSScriptRoot}/../../helm/monitoring"

try {
    $appEnvironment='local'

    helm upgrade --install `
        -n monitoring --create-namespace  `
        -f values.yaml `
        -f "values-$appEnvironment.yaml" `
        monitoring `
        .
    popd

    pushd "${PSScriptRoot}/../../helm/tracing-sample"
    helm upgrade --install `
        -n tracing-sample --create-namespace `
        -f values.yaml `
        -f "values-$appEnvironment.yaml" `
        tracing-sample `
        .
    
    echo "Open Grafana at http://localhost:8030"
}

finally {
    popd
}