FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src/Catalog.API

COPY ["Catalog.API/Catalog.API.csproj", "/src/Catalog.API/"]
RUN dotnet restore

COPY ["Catalog.API", "/src/Catalog.API/"]
RUN dotnet build --no-restore -c Release

FROM build AS publish
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=publish ["/src/Catalog.API/publish", "."]
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
