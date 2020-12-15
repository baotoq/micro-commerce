#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

WORKDIR /src
COPY ["BuildingBlocks/MicroCommerce.Shared/MicroCommerce.Shared.csproj", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Gateways/OcelotGateway.API/OcelotGateway.API.csproj", "Gateways/OcelotGateway.API/"]
RUN dotnet restore "Gateways/OcelotGateway.API/OcelotGateway.API.csproj"

COPY ["BuildingBlocks/MicroCommerce.Shared/", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Gateways/OcelotGateway.API/", "Gateways/OcelotGateway.API/"]

WORKDIR "/src/Gateways/OcelotGateway.API"
RUN dotnet build "OcelotGateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OcelotGateway.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcelotGateway.API.dll"]
