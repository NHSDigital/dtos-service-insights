FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Use the vendor-maintained Azure Functions base image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# Set environment variables (if these are relevant for all downstream containers)
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Remove unwanted libraries (e.g., Python)
RUN apt-get update && \
    apt-get purge -y python3 python3-minimal && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set a working directory (optional)
WORKDIR /home/site/wwwroot
