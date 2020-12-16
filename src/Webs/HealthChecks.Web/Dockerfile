#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

WORKDIR /src
COPY ["Webs/HealthChecks.Web/HealthChecks.Web.csproj", "Webs/HealthChecks.Web/"]
RUN dotnet restore "Webs/HealthChecks.Web/HealthChecks.Web.csproj"

COPY ["Webs/HealthChecks.Web/", "Webs/HealthChecks.Web/"]

WORKDIR "/src/Webs/HealthChecks.Web"
RUN dotnet build "HealthChecks.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthChecks.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthChecks.Web.dll"]
