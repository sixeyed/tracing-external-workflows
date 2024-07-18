
https://github.com/bitnami/charts/blob/main/bitnami/redis/README.md

Add and update the Bitnami repo:

```
helm repo add bitnami https://charts.bitnami.com/bitnami

helm repo update
```

Search for latest version:

```
helm search repo redis
```

Pull the desired version to download the chart locally:

```
cd helm\redis\charts

helm pull bitnami/redis --version 18.0.1
```
