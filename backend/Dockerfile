FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore "backend/PuckStats.Api.csproj"
RUN dotnet publish "backend/PuckStats.Api.csproj" -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=false
EXPOSE 8080
ENTRYPOINT ["dotnet", "PuckStats.Api.dll"]
