# b-shop

[![Build Status](https://github.com/bao2703/b-shop/workflows/.NET%20Core/badge.svg)](https://github.com/bao2703/b-shop/commits/master)

[[Sonar Gate]](https://baotoq-sonar.azurewebsites.net/projects)

| Project      | Status                                                                                                                                                                                               |
|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| identity-api | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/identity-api?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=6&branchName=master) |
| catalog-api  | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/catalog-api?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=5&branchName=master)  |
| react-web    | [![Build Status](https://bao2703.visualstudio.com/b-shop/_apis/build/status/react-web?branchName=master)](https://bao2703.visualstudio.com/b-shop/_build/latest?definitionId=7&branchName=master)    |


This is a simplified a commerce made in a micro-services architecture, using:

* .NET Core 3.1
* Internal communication using [Grpc](https://github.com/grpc/grpc-dotnet)
* SQL database with [MS SQL Server]()
* Accessing database with Entity Framework Core
* In-process messaging with [MediatR](https://github.com/jbogard/MediatR)
* A modern web application with [React](https://reactjs.org/)
* Logging with [Serilog](https://github.com/serilog/serilog)
* Identity and access management with [Identity server 4](http://docs.identityserver.io/en/latest/#)
* Building [Docker](https://www.docker.com/) images, managing containers
* [Docker compose](https://docs.docker.com/compose/)
* CI & CD with [Azure pipeline](https://azure.microsoft.com/en-us/services/devops/pipelines/)
* Orchestrating services using [Azure Kubernetes Service](https://azure.microsoft.com/en-us/services/kubernetes-service/)
* Deploy services to AKS by [helm 3](https://v3.helm.sh/)