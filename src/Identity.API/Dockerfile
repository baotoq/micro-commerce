FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ["Identity.API/Identity.API.csproj", "Identity.API/"]
COPY ["BuildingBlocks/UnitOfWork/UnitOfWork.csproj", "BuildingBlocks/UnitOfWork/"]
COPY ["BuildingBlocks/Shared/Shared.csproj", "BuildingBlocks/Shared/"]
RUN dotnet restore "Identity.API/Identity.API.csproj"

COPY ["Identity.API/", "/src/Identity.API/"]
COPY ["BuildingBlocks/UnitOfWork/", "BuildingBlocks/UnitOfWork/"]
COPY ["BuildingBlocks/Shared/", "BuildingBlocks/Shared/"]

WORKDIR /src/Identity.API
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=build ["/src/Identity.API/publish/", "."]
ENTRYPOINT ["dotnet", "Identity.API.dll"]
