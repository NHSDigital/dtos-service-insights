# ================================
#  Base Image Stage
# ================================
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# If you need any OS-level packages or custom tooling,
RUN apt-get update && \
    apt-get purge -y python3 python3-minimal && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

