#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

WORKDIR /src
COPY ["BuildingBlocks/MicroCommerce.Shared/MicroCommerce.Shared.csproj", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Identity/Shared/", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Identity/Webs/MicroCommerce.Identity.Web/MicroCommerce.Identity.Web.csproj", "Services/Identity/Webs/MicroCommerce.Identity.Web/"]
RUN dotnet restore "Services/Identity/Webs/MicroCommerce.Identity.Web/MicroCommerce.Identity.Web.csproj"

COPY ["BuildingBlocks/MicroCommerce.Shared/", "BuildingBlocks/MicroCommerce.Shared/"]
COPY ["Services/Identity/Webs/MicroCommerce.Identity.Web/", "Services/Identity/Webs/MicroCommerce.Identity.Web/"]

WORKDIR "/src/Services/Webs/Identity/MicroCommerce.Identity.Web"
RUN dotnet build "MicroCommerce.Identity.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroCommerce.Identity.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroCommerce.Identity.Web.dll"]
