# Micro Commerce

migrating to .net 8

| Project      | Azure                                                                                                                                                                                                |GitHub                                                                                                                                                                                                        |Sonar                                                                                                                                                                   |
|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| catalog-api  | [![Build Status](https://dev.azure.com/micro-commerce/micro-commerce/_apis/build/status/catalog-api?branchName=master)](https://dev.azure.com/micro-commerce/micro-commerce/_build/latest?definitionId=1&branchName=master) | [![catalog-api](https://github.com/baotoq/micro-commerce/actions/workflows/catalog-api.yml/badge.svg)](https://github.com/baotoq/micro-commerce/actions/workflows/catalog-api.yml)    | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=catalog-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=catalog-api)     |
| identity-api | [![Build Status](https://dev.azure.com/micro-commerce/micro-commerce/_apis/build/status/identity-api?branchName=master)](https://dev.azure.com/micro-commerce/micro-commerce/_build/latest?definitionId=2&branchName=master)| [![identity-api](https://github.com/baotoq/micro-commerce/actions/workflows/identity-api.yml/badge.svg)](https://github.com/baotoq/micro-commerce/actions/workflows/identity-api.yml) | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=identity-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=identity-api)   |
| ordering-api | [![Build Status](https://dev.azure.com/micro-commerce/micro-commerce/_apis/build/status/odering-api?branchName=master)](https://dev.azure.com/micro-commerce/micro-commerce/_build/latest?definitionId=3&branchName=master) |                                                                                                                                                                                       | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ordering-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=ordering-api)   |
| basket-api   | [![Build Status](https://dev.azure.com/micro-commerce/micro-commerce/_apis/build/status/basket-api?branchName=master)](https://dev.azure.com/micro-commerce/micro-commerce/_build/latest?definitionId=4&branchName=master)  |                                                                                                                                                                                       | [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=basket-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=basket-api)       |

Micro Commerce is a simple commercial application.
It demonstrates how to wire up small micro-services into a larger application using micro-services architectural principals.

## Technical stack

### Infrastructure

- **[`Docker for desktop (Kubernetes enabled)`](https://www.docker.com/products/docker-desktop)** - The easiest tool to run Docker, Docker Swarm and Kubernetes on Mac and Windows
- **[`Kubernetes`](https://kubernetes.io) / [`AKS`](https://docs.microsoft.com/en-us/azure/aks)** - The app is designed to run on Kubernetes (both locally on "Docker for Desktop" as well as on the cloud with AKS)
- **[`Helm`](https://helm.sh)** - Best package manager to find, share, and use software built for Kubernetes

### Front-end

- **[`TypeScript`](https://www.typescriptlang.org)** - A typed superset of JavaScript that compiles to plain JavaScript
- **[`Next.js`](https://nextjs.org)** - A modern server side rendering for React application
  
### Back-end

- **[`.NET Core 8`](https://dotnet.microsoft.com/download)** - .NET Framework and .NET Core, including ASP.NET and ASP.NET Core
- **[`EF Core 8`](https://github.com/dotnet/efcore)** - Modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations
- **[`FluentValidation`](https://github.com/FluentValidation/FluentValidation)** - Popular .NET validation library for building strongly-typed validation rules
- **[`MediatR`](https://github.com/jbogard/MediatR)** - Simple, unambitious mediator implementation in .NET
- **[`Serilog`](https://github.com/serilog/serilog)** - Simple .NET logging with fully-structured events

### CI & CD

- **[`Azure Pipelines`](https://azure.microsoft.com/en-us/services/devops/pipelines)**
- **[`GitHub Actions`](https://github.com/features/actions)**
- **[`SonarCloud`](https://sonarcloud.io/)**