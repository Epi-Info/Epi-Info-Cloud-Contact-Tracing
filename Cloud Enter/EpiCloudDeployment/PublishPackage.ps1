
#Note:The script help to publish the WebApp and WebAPI. 

[CmdletBinding(SupportsShouldProcess = $true)]
param(
	# The webSite Name you want to create
	[Parameter(Mandatory = $true)]
	[string]$WebSiteName, 

	# The webSite Job Type Name you want to create ‘triggered’ or ‘continuous’ Job 
	[string]$destination,

	# The full path to copy files from.
	[Parameter(Mandatory = $true)]
	[string]$WebPackagePath
)

#Login to portal 
$WebPackagePath = "C:\Users\rajaa\Desktop\ananthsite"
$destination = $source+".zip"

Add-Type -assembly "system.io.compression.filesystem"   
[io.compression.zipfile]::CreateFromDirectory($WebPackagePath,$destination) 
Publish-AzureWebsiteProject -Package $destination -Name $WebSiteName