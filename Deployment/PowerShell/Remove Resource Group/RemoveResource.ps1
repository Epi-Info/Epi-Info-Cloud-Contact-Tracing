param(
 [Parameter(Mandatory=$True)]
 [string]
 $subscriptionId,

 [Parameter(Mandatory=$True)]
 [string]
 $resourceGroupName
 
)

# sign in
Write-Host "Logging in...";
Login-AzureRmAccount;


# select subscription
Write-Host "Selecting subscription '$subscriptionId'";
Select-AzureRmSubscription -SubscriptionID $subscriptionId;


Write-Host "Removing Resource Group '$resourceGroupName'";
Remove-AzureRmResourceGroup $resourceGroupName


