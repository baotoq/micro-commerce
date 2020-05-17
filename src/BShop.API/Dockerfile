FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

COPY ["BShop.API/BShop.API.csproj", "/src/BShop.API/"]

WORKDIR /src/BShop.API
RUN dotnet restore

COPY ["BShop.API", "/src/BShop.API/"]
RUN dotnet build -c Release -o ./build

FROM build AS publish
RUN dotnet publish -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=publish /src/BShop.API/publish .
ENTRYPOINT ["dotnet", "BShop.API.dll"]
