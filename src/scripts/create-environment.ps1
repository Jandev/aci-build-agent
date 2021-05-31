# Inspiration: https://markheath.net/post/build-container-images-with-acr
# Sample: https://www.vivienfabing.com/azure-devops/2019/05/14/azure-pipelines-how-to-add-a-build-agent-with-azure-container-instances.html
# Another: https://blog.sluijsveld.com/27/09/2018/AzureContainerImageVnetAzureDevOps/
# First by Rene: https://roadtoalm.com/2017/08/18/running-a-linux-vsts-agent-on-azure-container-instances/

function Initialize-MicrosoftBuildAgent {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$resourceGroup,
        [Parameter(Mandatory)]
        [string]$buildagentContainerName,
        [Parameter(Mandatory)]
        [string]$azureDevOpsAccountName,
        [Parameter(Mandatory)]
        [string]$azureDevOpsToken,
        [Parameter(Mandatory=$false)]
        [string]$azureDevOpsPool = "Default",
        [Parameter(Mandatory=$false)]
        [string]$vnetName,
        [Parameter(Mandatory=$false)]
        [string]$subnetName
    )
    # Creating a build agent from the Microsoft registry
    if($vnetName -and $subnetName) {
        az container create -g $resourceGroup -n $buildagentContainerName `
            --image mcr.microsoft.com/azure-pipelines/vsts-agent `
            --cpu 2 `
            --memory 4 `
            --vnet-name $vnetName `
            --subnet $subnetName `
            --environment-variables VSTS_ACCOUNT=$azureDevOpsAccountName VSTS_TOKEN=$azureDevOpsToken VSTS_AGENT=$buildagentContainerName VSTS_POOL=$azureDevOpsPool
    } else {
        az container create -g $resourceGroup -n $buildagentContainerName `
            --image mcr.microsoft.com/azure-pipelines/vsts-agent `
            --cpu 2 `
            --memory 4 `
            --environment-variables VSTS_ACCOUNT=$azureDevOpsAccountName VSTS_TOKEN=$azureDevOpsToken VSTS_AGENT=$buildagentContainerName VSTS_POOL=$azureDevOpsPool
    }
}
$resourceGroup = "janv-containers"

# Creating the Resource Group
az group create -n $resourceGroup -l westeurope

$microsoftBuildAgentContainerName = "build-container"
$azureDevOpsPool = "Default"
$azureDevOpsAccountName = "janv"
$azureDevOpsToken = ""
Initialize-MicrosoftBuildAgent -resourceGroup $resourceGroup `
                                -buildagentContainerName $microsoftBuildAgentContainerName `
                                -azureDevOpsAccountName $azureDevOpsAccountName `
                                -azureDevOpsToken $azureDevOpsToken `
                                -azureDevOpsPool $azureDevOpsPool
# Stop the build agent
az container stop -g $resourceGroup -n $microsoftBuildAgentContainerName
# Start the build agent
az container start -g $resourceGroup -n $microsoftBuildAgentContainerName

$acrName = "janvregistry"
$vnetName = "janv-vnet"
$agentSubnet = "agent-subnet"
$agentSubnetPrefix = "10.0.2.0/27"
# Creating a Container Registry
az acr create -g $resourceGroup -n $acrName --sku Basic --admin-enabled
az network vnet create -g $resourceGroup -n $vnetName
az network vnet subnet create -g $resourceGroup `
    -n $agentSubnet `
    --vnet-name $vnetName `
    --address-prefixes $agentSubnetPrefix `
    --delegations "Microsoft.ContainerInstance.containerGroups"

Initialize-MicrosoftBuildAgent -resourceGroup $resourceGroup `
    -buildagentContainerName "$($microsoftBuildAgentContainerName)vnet" `
    -azureDevOpsAccountName $azureDevOpsAccountName `
    -azureDevOpsToken $azureDevOpsToken `
    -azureDevOpsPool $azureDevOpsPool