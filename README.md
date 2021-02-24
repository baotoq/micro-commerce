# Micro Commerce

| Project      | Azure                                                                                                                                                                                                |GitHub                                                                                                                                                       |Sonar                                                                                                                                                                            |
|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| identity-api | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/identity-api?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=6&branchName=master) |                                                                                                                                                             |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bshop-identity-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=bshop-identity-api) |
| catalog-api  | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/catalog-api?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=5&branchName=master)  | [![catalog-api](https://github.com/bao2703/b-shop/workflows/catalog-api/badge.svg)](https://github.com/bao2703/b-shop/actions?query=workflow%3Acatalog-api) |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bshop-catalog-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=bshop-catalog-api)   |
| react-web    | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/react-web?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=7&branchName=master)    |                                                                                                                                                             |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bshop-react-web&metric=alert_status)](https://sonarcloud.io/dashboard?id=bshop-react-web)       |

Micro Commerce is a simple commercial application.
It demonstrates how to wire up small micro-services into a larger application using micro-services architectural principals.

## Technical stack:

### Infrastructure
- **[`Docker for desktop (Kubernetes enabled)`](https://www.docker.com/products/docker-desktop)** - The easiest tool to run Docker, Docker Swarm and Kubernetes on Mac and Windows
- **[`Kubernetes`](https://kubernetes.io) / [`AKS`](https://docs.microsoft.com/en-us/azure/aks)** - The app is designed to run on Kubernetes (both locally on "Docker for Desktop" as well as on the cloud with AKS)
- **[`Helm`](https://helm.sh)** - Best package manager to find, share, and use software built for Kubernetes
- **[`Dapr`](https://dapr.io)** - An event-driven, portable runtime for building microservices on cloud and edge
- **[`Istio`](https://istio.io)** - for traffic management

### Front-end
- **[`TypeScript`](https://www.typescriptlang.org)** - A typed superset of JavaScript that compiles to plain JavaScript
- **[`Next.js`](https://nextjs.org)** - A modern server side rendering for React application
  
### Back-end
- **[`.NET Core 5`](https://dotnet.microsoft.com/download)** - .NET Framework and .NET Core, including ASP.NET and ASP.NET Core
- **[`EF Core 5`](https://github.com/dotnet/efcore)** - Modern object-database mapper for .NET. It supports LINQ queries, change tracking, updates, and schema migrations
- **[`IdentityServer4`](https://identityserver.io)** - Identity and Access Control solution for .NET Core
- **[`FluentValidation`](https://github.com/FluentValidation/FluentValidation)** - Popular .NET validation library for building strongly-typed validation rules
- **[`MediatR`](https://github.com/jbogard/MediatR)** - Simple, unambitious mediator implementation in .NET
- **[`Serilog`](https://github.com/serilog/serilog)** - Simple .NET logging with fully-structured events

### CI & CD
- **[`Azure Pipelines`](https://azure.microsoft.com/en-us/services/devops/pipelines)**
- **[`GitHub Actions`](https://github.com/features/actions)**
- **[`SonarCloud`](https://sonarcloud.io/)**
