FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY src/api/External.Api/External.Api.csproj ./api/External.Api/
RUN dotnet restore api/External.Api/External.Api.csproj

COPY src/api/External.Api/ ./api/External.Api/

ARG BUILD_COUNTER=0
RUN dotnet publish -c Release -o /out /property:Version=0.1.${BUILD_COUNTER}.0 api/External.Api/External.Api.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ENTRYPOINT ["dotnet", "/app/External.Api.dll"]
WORKDIR /app
COPY --from=builder /out/ .

ARG BUILD_ID=local
ARG COMMIT_SHA=local
LABEL build_id=${BUILD_ID}
LABEL commit_sha=${COMMIT_SHA}