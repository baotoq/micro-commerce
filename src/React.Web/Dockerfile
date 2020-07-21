FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
EXPOSE 80
EXPOSE 443

FROM node:alpine AS client
WORKDIR /src/React.Web/ClientApp
COPY ["React.Web/ClientApp/package.json", "."]
COPY ["React.Web/ClientApp/yarn.lock", "."]
RUN yarn config set strict-ssl false
RUN yarn install

COPY ["React.Web/ClientApp/", "."]
ARG REACT_APP_BASE_URI
ARG REACT_APP_IDENTITY_URI
ARG REACT_APP_CATALOG_URI
RUN yarn build

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

COPY ["React.Web/*.csproj", "React.Web/"]
RUN dotnet restore "React.Web/React.Web.csproj"

COPY ["React.Web/", "React.Web/"]
WORKDIR /src/React.Web
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-build -c Release -o ./publish

FROM base AS final
WORKDIR /app
COPY --from=client ["/src/React.Web/ClientApp/build/", "ClientApp/build/"]
COPY --from=build ["/src/React.Web/publish/", "."]
ENTRYPOINT ["dotnet", "React.Web.dll"]
