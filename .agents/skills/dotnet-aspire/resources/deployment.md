# Deploying .NET Aspire Applications

This guide covers deploying .NET Aspire applications from local development to production environments, including Azure Container Apps, Kubernetes, and other cloud platforms.

## Deployment Overview

.NET Aspire applications are designed for cloud deployment with built-in support for:
- Container orchestration
- Service discovery and communication
- Configuration management
- Observability and telemetry
- Scaling and resilience

## Deployment Targets

### Azure Container Apps (Recommended)

Azure Container Apps provides native support for .NET Aspire with automatic provisioning of Azure resources.

#### Prerequisites
- Azure subscription
- Azure CLI or Azure Developer CLI (azd)
- Docker Desktop (for building images)

#### Using Azure Developer CLI (azd)

**1. Initialize Azure Developer environment:**
```bash
azd init
```

Select "Use code in the current directory" and choose your AppHost project.

**2. Deploy to Azure:**
```bash
azd up
```

This command:
- Provisions Azure resources (Container Apps Environment, Storage, etc.)
- Builds and pushes container images to Azure Container Registry
- Deploys services to Azure Container Apps
- Configures service connections and environment variables

**3. Monitor deployment:**
```bash
azd monitor
```

Opens Application Insights for telemetry and logs.

**4. Manage environment:**
```bash
azd env list          # List environments
azd env set <name>    # Switch environment
azd down              # Tear down resources
```

#### Manual Azure Deployment

**1. Create Azure resources:**
```bash
# Resource group
az group create --name myapp-rg --location eastus

# Container Apps environment
az containerapp env create \
  --name myapp-env \
  --resource-group myapp-rg \
  --location eastus

# Container registry
az acr create \
  --name myappregistry \
  --resource-group myapp-rg \
  --sku Basic \
  --admin-enabled true
```

**2. Build and push images:**
```bash
# Build service image
docker build -t myappregistry.azurecr.io/apiservice:latest ./MyApi

# Login to ACR
az acr login --name myappregistry

# Push image
docker push myappregistry.azurecr.io/apiservice:latest
```

**3. Deploy container apps:**
```bash
az containerapp create \
  --name apiservice \
  --resource-group myapp-rg \
  --environment myapp-env \
  --image myappregistry.azurecr.io/apiservice:latest \
  --target-port 8080 \
  --ingress external \
  --registry-server myappregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password>
```

**4. Configure service connections:**
```bash
az containerapp update \
  --name webapp \
  --resource-group myapp-rg \
  --set-env-vars "services__apiservice__http__0=https://apiservice.internal.eastus.azurecontainerapps.io"
```

#### Azure Resource Provisioning

Aspire automatically provisions Azure resources when using `azd`:

**AppHost configuration:**
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Provisions Azure SQL Database in production
var sqlServer = builder.AddAzureSqlServer("sql")
    .AddDatabase("catalogdb");

// Provisions Azure Redis Cache in production
var cache = builder.AddAzureRedis("cache");

// Provisions Azure Service Bus in production
var messaging = builder.AddAzureServiceBus("messaging");

var apiService = builder.AddProject<Projects.Api>("apiservice")
    .WithReference(sqlServer)
    .WithReference(cache)
    .WithReference(messaging);

builder.Build().Run();
```

**Deployment behavior:**
- **Local development:** Uses containers (PostgreSQL, Redis, RabbitMQ)
- **Azure deployment:** Provisions managed Azure services (Azure SQL, Azure Cache for Redis, Azure Service Bus)

---

### Kubernetes

Deploy Aspire apps to Kubernetes clusters (AKS, EKS, GKE, on-premises).

#### Generate Kubernetes Manifests

**Using Aspire manifest:**
```bash
dotnet run --project MyApp.AppHost -- --publisher manifest --output-path ./aspire-manifest.json
```

**Convert to Kubernetes YAML:**
```bash
# Using aspirate tool
dotnet tool install -g aspirate

aspirate generate --aspire-manifest ./aspire-manifest.json --output-path ./k8s
```

This generates Kubernetes manifests including:
- Deployments
- Services
- ConfigMaps
- Secrets
- Ingress

#### Deploy to Kubernetes

**1. Apply manifests:**
```bash
kubectl apply -f ./k8s
```

**2. Verify deployment:**
```bash
kubectl get pods
kubectl get services
kubectl get ingress
```

**3. Configure service discovery:**

Aspire uses Kubernetes DNS for service discovery. Update service URLs:
```yaml
env:
  - name: services__apiservice__http__0
    value: "http://apiservice.default.svc.cluster.local"
```

#### Azure Kubernetes Service (AKS)

**1. Create AKS cluster:**
```bash
az aks create \
  --resource-group myapp-rg \
  --name myapp-cluster \
  --node-count 3 \
  --enable-addons monitoring \
  --generate-ssh-keys
```

**2. Get credentials:**
```bash
az aks get-credentials --resource-group myapp-rg --name myapp-cluster
```

**3. Deploy:**
```bash
kubectl apply -f ./k8s
```

**4. Enable Application Insights:**
```bash
az aks enable-addons \
  --resource-group myapp-rg \
  --name myapp-cluster \
  --addons monitoring \
  --workspace-resource-id <log-analytics-workspace-id>
```

---

### Docker Compose

For simple deployments or on-premises hosting.

#### Generate Docker Compose

**Using Aspire manifest:**
```bash
dotnet run --project MyApp.AppHost -- --publisher manifest --output-path ./aspire-manifest.json
```

**Convert to Docker Compose:**
```bash
# Manual conversion or use third-party tools
# Example docker-compose.yml:
```

```yaml
version: '3.8'

services:
  apiservice:
    build: ./MyApi
    ports:
      - "8080:8080"
    environment:
      - ConnectionStrings__catalogdb=Server=sqlserver;Database=catalogdb;User=sa;Password=YourPassword!
      - ConnectionStrings__cache=cache:6379
    depends_on:
      - sqlserver
      - cache

  webapp:
    build: ./MyWebApp
    ports:
      - "8000:8080"
    environment:
      - services__apiservice__http__0=http://apiservice:8080
    depends_on:
      - apiservice

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword!
    volumes:
      - sqldata:/var/opt/mssql

  cache:
    image: redis:latest
    volumes:
      - redisdata:/data

volumes:
  sqldata:
  redisdata:
```

#### Deploy with Docker Compose

```bash
docker-compose up -d
```

---

### Cloud Run (Google Cloud)

Deploy individual services to Cloud Run.

**1. Build and push images:**
```bash
gcloud builds submit --tag gcr.io/PROJECT_ID/apiservice ./MyApi
```

**2. Deploy service:**
```bash
gcloud run deploy apiservice \
  --image gcr.io/PROJECT_ID/apiservice \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated
```

**3. Configure service-to-service auth:**
```bash
gcloud run services add-iam-policy-binding apiservice \
  --member="serviceAccount:webapp@PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/run.invoker"
```

---

### AWS (ECS / App Runner)

Deploy to AWS Elastic Container Service or App Runner.

#### AWS App Runner

**1. Create ECR repository:**
```bash
aws ecr create-repository --repository-name apiservice
```

**2. Build and push:**
```bash
aws ecr get-login-password | docker login --username AWS --password-stdin <account>.dkr.ecr.us-east-1.amazonaws.com
docker build -t apiservice ./MyApi
docker tag apiservice:latest <account>.dkr.ecr.us-east-1.amazonaws.com/apiservice:latest
docker push <account>.dkr.ecr.us-east-1.amazonaws.com/apiservice:latest
```

**3. Create App Runner service:**
```bash
aws apprunner create-service \
  --service-name apiservice \
  --source-configuration "ImageRepository={ImageIdentifier=<account>.dkr.ecr.us-east-1.amazonaws.com/apiservice:latest,ImageRepositoryType=ECR}"
```

---

## Deployment Configurations

### Environment-Specific Settings

**appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Environment variables in deployment:**
```bash
# Azure Container Apps
az containerapp update \
  --name apiservice \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production"

# Kubernetes
env:
  - name: ASPNETCORE_ENVIRONMENT
    value: "Production"
```

### Connection String Management

**Azure Key Vault:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://mykeyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Kubernetes Secrets:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: connection-strings
type: Opaque
stringData:
  catalogdb: "Server=..."
  cache: "cache:6379"
```

**Reference in deployment:**
```yaml
env:
  - name: ConnectionStrings__catalogdb
    valueFrom:
      secretKeyRef:
        name: connection-strings
        key: catalogdb
```

### Service Discovery

**Azure Container Apps:**
Service discovery is automatic within the Container Apps environment. Use internal FQDNs:
```
https://<service-name>.internal.<region>.azurecontainerapps.io
```

**Kubernetes:**
Use cluster DNS:
```
http://<service-name>.<namespace>.svc.cluster.local
```

**Configure in service:**
```csharp
builder.Services.AddHttpClient("apiservice", client =>
{
    var serviceUrl = builder.Configuration["services:apiservice:http:0"]
                     ?? "http://apiservice";
    client.BaseAddress = new Uri(serviceUrl);
});
```

---

## Scaling and Performance

### Horizontal Scaling

**Azure Container Apps:**
```bash
az containerapp update \
  --name apiservice \
  --min-replicas 2 \
  --max-replicas 10 \
  --scale-rule-name http-rule \
  --scale-rule-type http \
  --scale-rule-metadata concurrentRequests=100
```

**Kubernetes:**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: apiservice-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: apiservice
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### Resource Limits

**Kubernetes:**
```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "250m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

**Azure Container Apps:**
```bash
az containerapp update \
  --name apiservice \
  --cpu 0.5 \
  --memory 1.0Gi
```

---

## Observability in Production

### Application Insights

**Configure in service:**
```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

**Azure Container Apps (automatic):**
```bash
az containerapp update \
  --name apiservice \
  --enable-dapr \
  --dapr-app-id apiservice
```

### Distributed Tracing

OpenTelemetry is configured by ServiceDefaults. Export to:
- Application Insights (Azure)
- Jaeger (self-hosted)
- Zipkin (self-hosted)
- Datadog, New Relic, etc.

**Configure OTLP exporter:**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        }));
```

### Logging

**Configure structured logging:**
```csharp
builder.Logging.AddJsonConsole();
```

**Azure Container Apps:** Logs automatically flow to Log Analytics

**Kubernetes:** Use Fluentd/Fluent Bit to ship logs to centralized storage

---

## Security Best Practices

### HTTPS/TLS

**Azure Container Apps:**
Automatic HTTPS with managed certificates.

**Kubernetes:**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: apiservice-ingress
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - api.example.com
    secretName: api-tls
```

### Secrets Management

- Use Azure Key Vault, AWS Secrets Manager, or GCP Secret Manager
- Never commit secrets to source control
- Use managed identities for cloud resources
- Rotate secrets regularly

### Network Security

**Azure Container Apps:**
- Enable network isolation
- Use Virtual Network integration
- Configure network security groups

**Kubernetes:**
```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: apiservice-policy
spec:
  podSelector:
    matchLabels:
      app: apiservice
  ingress:
  - from:
    - podSelector:
        matchLabels:
          app: webapp
```

---

## CI/CD Pipelines

### GitHub Actions (Azure)

```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Install azd
        run: curl -fsSL https://aka.ms/install-azd.sh | bash

      - name: Azure Login
        run: |
          azd auth login \
            --client-id "${{ secrets.AZURE_CLIENT_ID }}" \
            --client-secret "${{ secrets.AZURE_CLIENT_SECRET }}" \
            --tenant-id "${{ secrets.AZURE_TENANT_ID }}"

      - name: Deploy
        run: azd up --no-prompt
        env:
          AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
          AZURE_ENV_NAME: production
```

### Azure DevOps

```yaml
trigger:
  branches:
    include:
      - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- script: |
    curl -fsSL https://aka.ms/install-azd.sh | bash
  displayName: 'Install azd'

- task: AzureCLI@2
  inputs:
    azureSubscription: 'Azure-Connection'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      azd up --no-prompt
  env:
    AZURE_ENV_NAME: production
```

---

## Troubleshooting Production Issues

### Service Not Starting
- Check container logs: `kubectl logs <pod>` or Azure Portal
- Verify environment variables and connection strings
- Ensure container has proper resource limits
- Check health check endpoints

### Service Discovery Failing
- Verify service names match between AppHost and deployment
- Check DNS resolution: `nslookup <service-name>`
- Ensure network policies allow communication
- Review service mesh configuration if applicable

### Performance Issues
- Enable Application Insights or OpenTelemetry
- Review resource utilization (CPU, memory)
- Check database connection pooling
- Analyze distributed traces for bottlenecks
- Consider horizontal scaling

### Database Connection Failures
- Verify connection strings
- Check firewall rules
- Ensure managed identity permissions (Azure)
- Review network security groups
- Test connection from container: `kubectl exec -it <pod> -- /bin/bash`

---

## Deployment Checklist

Before deploying to production:

- [ ] Environment variables configured for production
- [ ] Connection strings stored in secrets/key vault
- [ ] HTTPS/TLS enabled
- [ ] Health checks configured
- [ ] Logging and telemetry enabled
- [ ] Resource limits and scaling rules defined
- [ ] Database migrations tested
- [ ] Backup and disaster recovery plan in place
- [ ] Monitoring and alerting configured
- [ ] Security scanning completed
- [ ] Load testing performed
- [ ] Rollback plan documented

---

## Additional Resources

- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [.NET Aspire Deployment](https://learn.microsoft.com/dotnet/aspire/deployment/overview)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)
