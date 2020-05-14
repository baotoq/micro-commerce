FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

COPY ["BShop.API/BShop.API.csproj", "src/BShop.API/"]

WORKDIR /src/BShop.API
RUN dotnet restore "BShop.API.csproj"

COPY ["BShop.API", "src/BShop.API/"]
RUN dotnet build "BShop.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BShop.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BShop.API.dll"]
