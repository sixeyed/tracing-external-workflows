
Install NSwag command line:

https://github.com/RicoSuter/NSwag/wiki/CommandLine


```
dotnet dotnet-nswag.dll openapi2csclient `
    /input:/Users/elton/scm/github/sixeyed/tracing-external-workflows/src/api/External.Api.Client/CodeGen/external-api-swagger.json `
    /classname:ExternalApiClient `
    /namespace:External.Api.Client `
    /output:/Users/elton/scm/github/sixeyed/tracing-external-workflows/src/api/External.Api.Client/CodeGen/ExternalApiClient.cs
```


nswag.json not used - can change client