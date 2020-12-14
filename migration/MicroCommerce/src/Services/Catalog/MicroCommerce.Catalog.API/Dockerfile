#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

WORKDIR /src
COPY ["BuildingBlocks/MicroCommerce.Shared/MicroCommerce.Shared.csproj", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Catalog/MicroCommerce.Catalog.API/MicroCommerce.Catalog.API.csproj", "Services/Catalog/MicroCommerce.Catalog.API/"]
RUN dotnet restore "Services/Catalog/MicroCommerce.Catalog.API/MicroCommerce.Catalog.API.csproj"

COPY ["BuildingBlocks/MicroCommerce.Shared/", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Catalog/MicroCommerce.Catalog.API/", "Services/Catalog/MicroCommerce.Catalog.API/"]
WORKDIR "/src/Services/Catalog/MicroCommerce.Catalog.API"

RUN dotnet build "MicroCommerce.Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroCommerce.Catalog.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroCommerce.Catalog.API.dll"]
