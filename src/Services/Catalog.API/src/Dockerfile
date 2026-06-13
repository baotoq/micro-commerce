# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# csproj-first for layer caching
COPY Api/MicroCommerce.Catalog.csproj                           Api/
COPY Application/MicroCommerce.Catalog.Application.csproj        Application/
COPY Infrastructure/MicroCommerce.Catalog.Infrastructure.csproj Infrastructure/
COPY Domain/MicroCommerce.Catalog.Domain.csproj                 Domain/
RUN dotnet restore Api/MicroCommerce.Catalog.csproj
COPY . .
RUN dotnet publish Api/MicroCommerce.Catalog.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
LABEL org.opencontainers.image.source=https://github.com/baotoq/micro-commerce
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MicroCommerce.Catalog.dll"]
