
pushd "${PSScriptRoot}/../../src/tools/Tracing.WorkflowGenerator"

try {
    $env:TracingSample__WorkflowGenerator__WorkflowCount='600'
    $env:TracingSample__WorkflowGenerator__BatchSize='200'
    $env:TracingSample__WorkflowGenerator__BatchWaitMinutes='10'

    $env:TracingSample__Redis__ConnectionString='localhost:8379'

    dotnet run
}
finally {
    popd
}
