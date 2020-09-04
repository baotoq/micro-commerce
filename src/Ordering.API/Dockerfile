FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY ["Odering.API/*.csproj", "Odering.API/"]
COPY ["BuildingBlocks/Data.Entities/*.csproj", "BuildingBlocks/Data.Entities/"]
COPY ["BuildingBlocks/Data.UnitOfWork.Dapper/*.csproj", "BuildingBlocks/Data.UnitOfWork.Dapper/"]
COPY ["BuildingBlocks/Data.UnitOfWork.EF/*.csproj", "BuildingBlocks/Data.UnitOfWork.EF/"]
COPY ["BuildingBlocks/Shared/*.csproj", "BuildingBlocks/Shared/"]
COPY ["BuildingBlocks/Shared.Grpc/*.csproj", "BuildingBlocks/Shared.Grpc/"]
COPY ["Protos/", "Protos/"]
RUN dotnet restore "Odering.API/Odering.API.csproj"

COPY ["Odering.API/", "Odering.API/"]
COPY ["BuildingBlocks/Data.Entities/", "BuildingBlocks/Data.Entities/"]
COPY ["BuildingBlocks/Data.UnitOfWork.Dapper/", "BuildingBlocks/Data.UnitOfWork.Dapper/"]
COPY ["BuildingBlocks/Data.UnitOfWork.EF/", "BuildingBlocks/Data.UnitOfWork.EF/"]
COPY ["BuildingBlocks/Shared/", "BuildingBlocks/Shared/"]
COPY ["BuildingBlocks/Shared.Grpc/", "BuildingBlocks/Shared.Grpc/"]

WORKDIR /src/Odering.API
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=build ["/src/Odering.API/publish/", "."]
ENTRYPOINT ["dotnet", "Odering.API.dll"]
