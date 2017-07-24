#Publish Web App and API
#Step 1:Create Package.
#Step 2:Run the script and run time give the path of the package.

[CmdletBinding(SupportsShouldProcess = $true)]
param(

	#Enter the subscription Id
	[Parameter(Mandatory=$true)]
	[string]$SubscribId,

	# The webSite Name you want to create
	[Parameter(Mandatory = $true)]
	[string]$WebSiteName, 

	# Enter the Publish Folder path
	[string]$destination,

	# The full path to copy files from.
	[Parameter(Mandatory = $true)]
	[string]$WebPackagePath
)

Write-Host "Enter the Subscription Id"
$SubscribId

Write-Host "Enter the WebSiteName"
$WebSiteName

Write-Host "Enter the Publish folder path"
$destination


#Login-AzureRmAccount

Write-Host "Package Zip is created in the path"

Select-AzureSubscription -SubscriptionId $SubscribId

#Login to portal  
$destination = $WebPackagePath+".zip"



Try
{ 
		 
			# Start the deployment
			
			Write-Host "Convert publish folder to Zip:"+$WebSiteName;
			Add-Type -assembly "system.io.compression.filesystem"   
			[io.compression.zipfile]::CreateFromDirectory($WebPackagePath,$destination)
			
			Write-Host "Publishing To :"$WebSiteName; 
			#Publish-AzureWebsiteProject -Package $destination -Name $WebSiteName
			Write-Host "Publish Completed"
			Show-AzureWebsite -Name $websiteName
	
   
}
Catch
{
	Write-Host $_.Exception.Message -ForegroundColor Yellow

	 Write-Host "Removing the resource group '$resourceGroupName'";
}