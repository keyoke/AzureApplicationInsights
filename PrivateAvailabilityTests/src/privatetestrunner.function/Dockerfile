FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

WORKDIR /privatetestrunner/
COPY . ./

RUN mkdir -p /home/site/wwwroot && \
    dotnet publish privatetestrunner.function --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:3.0-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:3.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=build ["/home/site/wwwroot", "/home/site/wwwroot"]