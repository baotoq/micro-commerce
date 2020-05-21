FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src/Identity.API

COPY ["Identity.API/Identity.API.csproj", "/src/Identity.API/"]
RUN dotnet restore

COPY ["Identity.API", "/src/Identity.API/"]
RUN dotnet build --no-restore -c Release

FROM build AS publish
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=publish ["/src/Identity.API/publish", "."]
ENTRYPOINT ["dotnet", "Identity.API.dll"]
