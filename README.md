# ondotnet-observability 

Samples for HealthCheck, Monitoring and Feature Management
---------------------------------------------------------

## Demo Recording 

- https://www.youtube.com/watch?v=PDdHa0ushJ0

## Run the sample locally

- Navigate to the `microservices` folder in a CLI. Then run the command `tye run`. Tye dashboard will be available at `http://127.0.0.1:8000/`

    ![](/imgs/tye-local-run.png)

If you're new to Tye, checkout the [getting started](https://github.com/dotnet/tye/blob/main/docs/getting_started.md) guide first.

>!NOTE: You need to have docker installed in your local developerment environment. For more details refer [docker-desktop](https://www.docker.com/products/docker-desktop)

## Deploy the sample to Azure

### Prerequisite

Before you plan to deploy the application in Azure you need to make sure you have set up the following service instances.

#### Azure Command-Line Interface 

- The Azure CLI is a great tool to work with Azure resources.
- If you haven't installed it yet, then [Install the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

> !NOTE: Below instructions are from the Azure Portal. You can similarly use `Azure CLI` to create those specific service instances with respective commands. The entire list can be found in [az commands](https://docs.microsoft.com/en-us/cli/azure/reference-index?view=azure-cli-latest)

#### Resource Groups

- You need to have a resource group for all your Azure service instances.
- So better to create one at the beginning. You can do that by following [create a resource groups](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal#create-resource-groups)

#### Application Insights Resource

- All logs and traces in Azure, are captured in Application Insights.
- You need to [Create an Application Insights resource](https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource) and copy the instrumentation key.
- And then replace the **`<AppInsights-Instrumentation-Key>`** in `tye.yaml` with the previously copied instrumentation key.

    ![](/imgs/appinsights.png)


#### Azure Maps Account 

- You will need to create a [Azure Maps Account](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-manage-account-keys)
- And replace **`<Azure-Map-Subscription-Key>`** in `tye.yaml` with the **`Primary Key`** mentioned in [Manage authentication in Azure Maps](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-manage-authentication)

    ![](/imgs/azure-map-account.png)


#### App Configuration
- In production these app uses `FeatureManagement` which is configured as Feature Manager of `Azure App Configuration` 
- You can follow this guide [Create an App Configuration store](https://docs.microsoft.com/en-us/azure/azure-app-configuration/quickstart-feature-flag-aspnet-core?tabs=core5x#create-an-app-configuration-store) to configure it easily. 
- You need to make sure you are configuring following `feature flags` in there, with the exact same name. 
    - `StaticWeatherAPI`
    - `ExternalWeatherAPI`
- You can keep one of the features enabled based on your choice. 
- Once you have configured this, you need to replace `<App-Config-Endpoint>` value with respective `AppConfig Endpoint ConnectionString`. It looks something like `Endpoint=<AppConfig-Endpoint>Secret=<Secret>`

    ![](/imgs/feature-manager.png)


#### Azure Container Registry (ACR)

- You need to [Create a container registry](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal#create-a-container-registry) to store all your docker images for the Kubernetes cluster.

    ![](/imgs/acr-repos.png)


#### Azure Kubernetes Service (AKS)

- You need to [Create an AKS cluster](https://docs.microsoft.com/en-us/azure/aks/kubernetes-walkthrough-portal#create-an-aks-cluster)
- You need to make sure that you have configured the existing `Container Registry` during the Kubernetes cluster creation step. But in case if you have missed out that you can still [configure ACR integration for existing AKS clusters](https://docs.microsoft.com/en-us/azure/aks/cluster-container-registry-integration#configure-acr-integration-for-existing-aks-clusters)
- Once the cluster is up and running, you need to configure the kubectl to connect to your Kubernetes cluster using the `az aks get-credentials` command.

    ![](/imgs/monitor-aks.PNG)


### Deploy all required components to k8s

- Go to the directory `ondotnet-observability/microservices`
- Run the `.\deploy-pre.ps1` script to deploy *Redis*, *Zipkin*, and *Seq* related deployment and services.
- All the relevant manifest files are kept under the `deploy/k8s` directory.

### Use tye deploy

- Now, you have all the components to set up and run the `tye deploy -i` command.
- Tye will ask for the following information which you need to provide as mentioned below :
    - `Container registry` : `<registry-given-name>.azurecr.io`
    - `Redis Connection String`:  `redis:6379`
    - `Seq Url` : `http://seq`
    - `Zipkin Url`: `http://zipkin:9411` 

- After successful deployment, you can make sure if everything is up and running using the command `kubectl get pods`

    ![](/imgs/kubectl-get-pods.png)

- And you can look at all the running services using the command `kubectl get svc`

    ![](/imgs/kubectl-get-svcs.png)

- To access a specific application you need to forward that service port to one of your local port and browse that url. 
For e.g : if you want to access `frontend` service, you can use `kubectl port-forward svc/frontend 9001:80` and browse the `frontend` app at `http://localhost:9001`

>!NOTE : In real scenarios with kubernetes deployment you may want to use either ingress or service type of loadbalancer to access an application from the outside of the cluster. For more details, you can refer [Ingress](https://kubernetes.io/docs/concepts/services-networking/ingress/) api object.    

### Azure Dashboard

- You can create your own custom dashboard to monitor different aspects of your infra and application. Just like how it has been shown below :

    ![](/imgs/azure-dashboard.png)


## Additional Resources

- [Learn Cloud-Native Architecture and Microservices](https://dotnet.microsoft.com/learn/microservices)
- [Feature management overview](https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management)
- [Request real-time and forecasted weather data using Azure Maps Weather services](https://docs.microsoft.com/en-us/azure/azure-maps/how-to-request-weather-data)
- [Azure App Configuration documentation](https://docs.microsoft.com/en-us/azure/azure-app-configuration/)
- [What is Application Insights?](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure Container Registry](https://azure.microsoft.com/en-in/services/container-registry/)
- [Kubernetes core concepts for Azure Kubernetes Service (AKS)](https://docs.microsoft.com/en-us/azure/aks/concepts-clusters-workloads)
