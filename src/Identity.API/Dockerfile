FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ["Identity.API/*.csproj", "Identity.API/"]
COPY ["BuildingBlocks/Data.Entities/*.csproj", "BuildingBlocks/Data.Entities/"]
COPY ["BuildingBlocks/Data.UnitOfWork/*.csproj", "BuildingBlocks/Data.UnitOfWork/"]
COPY ["BuildingBlocks/Data.UnitOfWork.EF/*.csproj", "BuildingBlocks/Data.UnitOfWork.EF/"]
COPY ["BuildingBlocks/Shared/*.csproj", "BuildingBlocks/Shared/"]
COPY ["BuildingBlocks/Shared.Grpc/*.csproj", "BuildingBlocks/Shared.Grpc/"]
COPY ["Protos/", "Protos/"]
RUN dotnet restore "Identity.API/Identity.API.csproj"

COPY ["Identity.API/", "Identity.API/"]
COPY ["BuildingBlocks/Data.Entities/", "BuildingBlocks/Data.Entities/"]
COPY ["BuildingBlocks/Data.UnitOfWork/", "BuildingBlocks/Data.UnitOfWork/"]
COPY ["BuildingBlocks/Data.UnitOfWork.EF/", "BuildingBlocks/Data.UnitOfWork.EF/"]
COPY ["BuildingBlocks/Shared/", "BuildingBlocks/Shared/"]
COPY ["BuildingBlocks/Shared.Grpc/", "BuildingBlocks/Shared.Grpc/"]

WORKDIR /src/Identity.API
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=build ["/src/Identity.API/publish/", "."]
ENTRYPOINT ["dotnet", "Identity.API.dll"]
