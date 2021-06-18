#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY ["BuildingBlocks/MicroCommerce.Shared/MicroCommerce.Shared.csproj", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Basket/MicroCommerce.Basket.API/MicroCommerce.Basket.API.csproj", "Services/Basket/MicroCommerce.Basket.API/"]
RUN dotnet restore "Services/Basket/MicroCommerce.Basket.API/MicroCommerce.Basket.API.csproj"

COPY ["BuildingBlocks/MicroCommerce.Shared/", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Basket/MicroCommerce.Basket.API/", "Services/Basket/MicroCommerce.Basket.API/"]

WORKDIR "/src/Services/Basket/MicroCommerce.Basket.API"
RUN dotnet build "MicroCommerce.Basket.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroCommerce.Basket.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroCommerce.Basket.API.dll"]
