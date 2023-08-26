FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /build
COPY ["src/Api/CartService/CartService.csproj", "src/Api/CartService/"]
COPY ["src/Api/ProductService/ProductService.csproj", "src/Api/ProductService/"]

COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]

COPY ["tests/Application.Tests/Application.Tests.csproj", "tests/Application.Tests/"]

COPY ["MicroCommerce.sln", "MicroCommerce.sln"]

RUN dotnet restore

COPY ["src/", "src/"]
COPY ["tests/", "tests/"]

RUN dotnet build -c Release -o /app/build

FROM build AS publish
ARG SERVICE_NAME
RUN ls
WORKDIR "/build/src/Api/${SERVICE_NAME}"
RUN dotnet publish "${SERVICE_NAME}.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG SERVICE_NAME
ENV SERVICE_NAME=$SERVICE_NAME
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT dotnet ${SERVICE_NAME}.dll
