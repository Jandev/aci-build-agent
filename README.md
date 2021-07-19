# Builds by build status
https://docs.microsoft.com/en-us/rest/api/azure/devops/build/builds/list?view=azure-devops-rest-6.0#buildstatus

Request:
{{baseUrl}}/build/builds?statusFilter=notStarted&api-version=6.0
Response:
{
    "count": 0,
    "value": []
}

# Releases list with environment statusses
https://vsrm.{{instance}}/{{teamproject}}/_apis/release/releases?$expand=environments&api-version=6.1-preview.8

```
environments": [
                {
                    "id": 70
         "deploySteps": [
                        {
                            // https://docs.microsoft.com/en-us/rest/api/azure/devops/release/releases/list?view=azure-devops-rest-6.0#deploymentoperationstatus
                            "status": "succeeded", // "queued", "queuedForAgent", "queuedForPipeline"
                            "hasStarted": true,
```