# b-shop

![catalog-api](https://github.com/bao2703/b-shop/workflows/catalog-api/badge.svg)

| Project      | Azure                                                                                                                                                                                                |GitHub                                                                             |Sonar                                                                                                                                                                            |
|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| identity-api | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/identity-api?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=6&branchName=master) |![catalog-api](https://github.com/bao2703/b-shop/workflows/catalog-api/badge.svg)  |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bshop-identity-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=bshop-identity-api) |
| catalog-api  | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/catalog-api?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=5&branchName=master)  |                                                                                   |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bshop-catalog-api&metric=alert_status)](https://sonarcloud.io/dashboard?id=bshop-catalog-api)   |
| react-web    | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/react-web?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=7&branchName=master)    |                                                                                   |[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bshop-react-web&metric=alert_status)](https://sonarcloud.io/dashboard?id=bshop-react-web)       |

This is a simplified a commerce made in a micro-services architecture, using:

* .NET Core 3.1
* Internal communication using [Grpc](https://github.com/grpc/grpc-dotnet)
* SQL database with [MS SQL Server](), [PostgreSQL]()
* Accessing database with [Entity Framework Core](), [Dapper]()
* In-process messaging with [MediatR](https://github.com/jbogard/MediatR)
* A modern web application with [React](https://reactjs.org)
* Logging with [Serilog](https://github.com/serilog/serilog)
* Identity and access management with [Identity server 4](http://docs.identityserver.io/en/latest)
* Building [Docker](https://www.docker.com/) images, managing containers
* [Docker compose](https://docs.docker.com/compose/)
* CI & CD with [Azure pipeline](https://azure.microsoft.com/en-us/services/devops/pipelines/)
* Orchestrating services using [Azure Kubernetes Service](https://azure.microsoft.com/en-us/services/kubernetes-service)
* Deploy services to AKS by [helm 3](https://v3.helm.sh)
* [Istio(https://istio.io) for traffic management
