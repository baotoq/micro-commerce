FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ["Basket.API/*.csproj", "Basket.API/"]
COPY ["BuildingBlocks/Shared/*.csproj", "BuildingBlocks/Shared/"]
COPY ["BuildingBlocks/Shared.Grpc/*.csproj", "BuildingBlocks/Shared.Grpc/"]
COPY ["Protos/", "Protos/"]
RUN dotnet restore "Basket.API/Basket.API.csproj"

COPY ["Basket.API/", "Basket.API/"]
COPY ["BuildingBlocks/Shared/", "BuildingBlocks/Shared/"]
COPY ["BuildingBlocks/Shared.Grpc/", "BuildingBlocks/Shared.Grpc/"]

WORKDIR /src/Basket.API
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=build ["/src/Basket.API/publish/", "."]
ENTRYPOINT ["dotnet", "Basket.API.dll"]
