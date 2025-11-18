# Use .NET 9.0 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SabidosAPI-Core.csproj", "."]
RUN dotnet restore "./SabidosAPI-Core.csproj"
COPY . .
RUN dotnet build "./SabidosAPI-Core.csproj" -c Release -o /app/build

# Use .NET 9.0 runtime para execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM build AS publish
RUN dotnet publish "./SabidosAPI-Core.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SabidosAPI-Core.dll"]