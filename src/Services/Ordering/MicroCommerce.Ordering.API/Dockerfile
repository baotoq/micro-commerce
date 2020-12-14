#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

WORKDIR /src
COPY ["BuildingBlocks/MicroCommerce.Shared/MicroCommerce.Shared.csproj", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Ordering/MicroCommerce.Ordering.API/MicroCommerce.Ordering.API.csproj", "Services/Ordering/MicroCommerce.Ordering.API/"]
RUN dotnet restore "Services/Ordering/MicroCommerce.Ordering.API/MicroCommerce.Ordering.API.csproj"

COPY ["BuildingBlocks/MicroCommerce.Shared/", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Ordering/MicroCommerce.Ordering.API/", "Services/Ordering/MicroCommerce.Ordering.API/"]

WORKDIR "/src/Services/Ordering/MicroCommerce.Ordering.API"
RUN dotnet build "MicroCommerce.Ordering.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroCommerce.Ordering.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroCommerce.Ordering.API.dll"]
