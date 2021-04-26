FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build

WORKDIR /privatetestrunner/
COPY . ./

RUN dotnet publish privatetestrunner.console -c Release --output output

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS runtime
COPY --from=build /privatetestrunner/output .
ENTRYPOINT ["dotnet", "privatetestrunner.console.dll"]