$imageName = "dockeragent"
$imageVersion = "v1"

$acrName = "janvregistry"
$acrHostname = "$($acrName).azurecr.io"
$adminUsername = "janvregistry"
$adminPassword = "" # The password of your Container Registry

# Build the Docker image from the folder `dockeragent`
docker build -t "$($imageName):latest" -t "$($imageName):$($imageVersion)" .\dockeragent
# Tag the image with the ACR hostname
docker tag "$($imageName):$($imageVersion)" "$($acrHostname)/$($imageName):$($imageVersion)"
docker tag "$($imageName):latest" "$($acrHostname)/$($imageName):latest"
# Login to ACR
az acr login --name janvregistry --username $adminUsername --password $adminPassword
# Push the images to ACR
docker push "$($acrHostname)/$($imageName):$($imageVersion)"
docker push "$($acrHostname)/$($imageName):latest"
# Remove the local ACR tagged images
docker rmi "$($acrHostname)/$($imageName):$($imageVersion)"
docker rmi "$($acrHostname)/$($imageName):latest"
