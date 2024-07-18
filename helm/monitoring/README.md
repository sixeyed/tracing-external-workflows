Sources:

- Grafana (dashboard UI): https://github.com/grafana/helm-charts/tree/main/charts/grafana

- Tempo (trace collection & storage): https://github.com/grafana/helm-charts/tree/main/charts/tempo

Add and update the Bitnami repo:

```
helm repo add grafana https://grafana.github.io/helm-charts

helm repo update
```

Search for latest version:

```
helm search repo tempo
```

Pull the desired version to download the chart locally:

```
cd helm\monitoring\charts

helm pull grafana/tempo --version 1.7.2

helm pull grafana/grafana --version 7.3.9
```


Prometheus:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts

helm repo update

helm search repo prometheus

helm pull prometheus-community/prometheus --version 25.20.0
```