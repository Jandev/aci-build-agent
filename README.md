# Overview

This repository contains the scripts to create a Docker image that can act as an Azure DevOps agent.  
It also contains scripts to create an Azure Container Instance environment & Azure Container Registry.

The Azure Function project acts as an orchestrator to start and stop the ACI based on the information it can retrieve from Azure DevOps.

# Some useful URLs

## Azure DevOps

### Builds by build status
https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-6.0#buildstatus 

Request:
```
{{baseUrl}}/build/builds?statusFilter=notStarted&api-version=6.0
```
Response:
```json
{
    "count": 0,
    "value": []
}
```

### Releases list with environment statuses

https://vsrm.{{instance}}/{{teamproject}}/_apis/release/releases?$expand=environments&api-version=6.1-preview.8

```json
{
    "environments": [
        {
            "id": 70,
            "deploySteps": [
                {
                    // https://docs.microsoft.com/en-us/rest/api/azure/devops/release/releases/list?view=azure-devops-rest-6.0#deploymentoperationstatus
                    "status": "succeeded", // "queued", "queuedForAgent", "queuedForPipeline"
                    "hasStarted": true,
                }
            ]
}
```

## GitHub
Opened 
* [List Releases, not all URI parameters work #485](https://github.com/MicrosoftDocs/vsts-rest-api-specs/issues/485)
 

## Blogposts
Scaling isn't possible in ACI.  
Tom Kerkhove has an interesting post on this: https://blog.tomkerkhove.be/2021/01/02/autoscaling-azure-container-instances-with-azure-serverless/

Related blogposts of myself:
* [Create a private build agent using Azure Container Instances](https://jan-v.nl/post/2021/create-build-agent-with-azure-container-instances/)