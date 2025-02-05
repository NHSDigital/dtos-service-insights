FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Copy in your .csproj and source files
COPY ./BIAnalyticsDataService/CreateParticipantScreeningEpisode /src/dotnet-function-app
COPY ./Shared /Shared

# Publish to /home/site/wwwroot
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot


# # Instead of pulling from the MS base, we use your custom base image
# #FROM myregistry.azurecr.io/dotnet-function-base:8.0
FROM localhost:5000/service-insights-base-image:latest

# Copy the compiled output from the installer-env stage
COPY --from=installer-env /home/site/wwwroot /home/site/wwwroot


