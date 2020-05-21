FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM node:12-alpine AS client
WORKDIR /src
COPY ["React.Web/ClientApp", "."]
RUN npm install

ARG REACT_APP_BASE_URI
ARG REACT_APP_IDENTITY_URI
ARG REACT_APP_RESOURCE_URI
RUN npm run build

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src/React.Web

COPY ["React.Web/React.Web.csproj", "/src/React.Web/"]
RUN dotnet restore

COPY ["React.Web", "/src/React.Web/"]
RUN dotnet build --no-restore -c Release

FROM build AS publish
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=client ["/src/build", "ClientApp/build"]
COPY --from=publish ["/src/React.Web/publish", "."]
ENTRYPOINT ["dotnet", "React.Web.dll"]
