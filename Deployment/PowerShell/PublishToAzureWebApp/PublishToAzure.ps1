param(
 [Parameter(Mandatory=$True)]
 [string]
 $subscriptionId,

 [Parameter(Mandatory=$True)]
 [string]
 $resourceGroupName,

 [string]
 $resourceGroupLocation,

 [Parameter(Mandatory=$True)]
 [string]
 $WebAppName,

 
 [Parameter(Mandatory=$True)]
 [string]
 $WebAppLocation,

 [string]
 $PublishSettingFileLocation = "C:\Users\PatelH\Downloads\Visual Studio Enterprise_ BizSpark-12-30-2016-credentials.publishsettings" 
 
)

# sign in
Write-Host "Logging in...";
Login-AzureRmAccount;


Import-AzurePublishSettingsFile $PublishSettingFileLocation

# select subscription
Write-Host "Selecting subscription '$subscriptionId'";
Select-AzureRmSubscription -SubscriptionID $subscriptionId;


#Create or check for existing resource group
$resourceGroup = Get-AzureRmResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
if(!$resourceGroup)
{
    Write-Host "Resource group '$resourceGroupName' does not exist. To create a new resource group, please enter a location.";
    if(!$resourceGroupLocation) {
        $resourceGroupLocation = Read-Host "resourceGroupLocation";
    }
    Write-Host "Creating resource group '$resourceGroupName' in location '$resourceGroupLocation'";
    New-AzureRmResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
}
else{
    Write-Host "Using existing resource group '$resourceGroupName'";
}


Get-AzureWebsite -Name $WebAppName

Publish-AzureWebsiteProject -Name $WebAppName -Package $WebAppLocation
