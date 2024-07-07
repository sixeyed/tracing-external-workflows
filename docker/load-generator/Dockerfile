ARG DOTNET_VERSION
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-alpine AS builder

WORKDIR /src
COPY src/messaging/Common.Messaging/Common.Messaging.csproj ./messaging/Common.Messaging/
COPY src/tools/Tracing.WorkflowGenerator/Tracing.WorkflowGenerator.csproj ./tools/Tracing.WorkflowGenerator/
RUN dotnet restore tools/Tracing.WorkflowGenerator/Tracing.WorkflowGenerator.csproj

COPY src/messaging/Common.Messaging/ ./messaging/Common.Messaging/
COPY src/tools/Tracing.WorkflowGenerator/ ./tools/Tracing.WorkflowGenerator/

ARG BUILD_COUNTER=0
RUN dotnet publish -c Release -o /out /property:Version=0.1.${BUILD_COUNTER}.0 tools/Tracing.WorkflowGenerator/Tracing.WorkflowGenerator.csproj

# app image
ARG DOTNET_VERSION
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine

ENV TracingSample__WorkflowGenerator__WorkflowCount='100' \
    TracingSample__WorkflowGenerator__BatchSize='20' \
    TracingSample__WorkflowGenerator__BatchWaitMinutes='2' \
    TracingSample__Redis__ConnectionString='redis:8379'

ENTRYPOINT ["dotnet", "/app/Tracing.WorkflowGenerator.dll"]
WORKDIR /app
COPY --from=builder /out/ .

ARG BUILD_ID=local
ARG COMMIT_SHA=local
LABEL build_id=${BUILD_ID}
LABEL commit_sha=${COMMIT_SHA}