FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY . ./privatetestrunner
WORKDIR /privatetestrunner/
RUN dotnet build -c Release -o output

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime
COPY --from=build /privatetestrunner/output .
ENTRYPOINT ["dotnet", "privatetestrunner.dll"]