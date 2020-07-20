FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ["Catalog.API/*.csproj", "Catalog.API/"]
COPY ["BuildingBlocks/UnitOfWork/*.csproj", "BuildingBlocks/UnitOfWork/"]
COPY ["BuildingBlocks/Shared/*.csproj", "BuildingBlocks/Shared/"]
COPY ["Protos/", "Protos/"]
RUN dotnet restore "Catalog.API/Catalog.API.csproj"

COPY ["Catalog.API/", "Catalog.API/"]
COPY ["BuildingBlocks/UnitOfWork/", "BuildingBlocks/UnitOfWork/"]
COPY ["BuildingBlocks/Shared/", "BuildingBlocks/Shared/"]

WORKDIR /src/Catalog.API
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=build ["/src/Catalog.API/publish/", "."]
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
