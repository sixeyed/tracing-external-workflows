ARG DOTNET_VERSION
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS builder

WORKDIR /src
COPY src/messaging/Common.Messaging/Common.Messaging.csproj ./messaging/Common.Messaging/
COPY src/api/External.Api.Client/External.Api.Client.csproj ./api/External.Api.Client/
COPY src/worker/Tracing.Worker/Tracing.Worker.csproj ./worker/Tracing.Worker/
RUN dotnet restore worker/Tracing.Worker/Tracing.Worker.csproj

COPY src/messaging/Common.Messaging/ ./messaging/Common.Messaging/
COPY src/api/External.Api.Client/ ./api/External.Api.Client/
COPY src/worker/Tracing.Worker/ ./worker/Tracing.Worker/

ARG BUILD_COUNTER=0
RUN dotnet publish -c Release -o /out /property:Version=0.1.${BUILD_COUNTER}.0 worker/Tracing.Worker/Tracing.Worker.csproj

# app image
ARG DOTNET_VERSION
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine

ENTRYPOINT ["dotnet", "/app/Tracing.Worker.dll"]
WORKDIR /app
COPY --from=builder /out/ .

ARG BUILD_ID=local
ARG COMMIT_SHA=local
LABEL build_id=${BUILD_ID}
LABEL commit_sha=${COMMIT_SHA}