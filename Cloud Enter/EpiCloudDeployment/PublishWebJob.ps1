[CmdletBinding(SupportsShouldProcess = $true)]
param(
# The name of valide Azure Subscription associated with Account
[Parameter(Mandatory = $true)]
[string]$AzureSubscriptionName,

# The webSite Name you want to create
[Parameter(Mandatory = $true)]
[string]$WebSiteName,

# The webSite Job Name you want to create
[Parameter(Mandatory = $true)]
[string]$WebSiteJobName,

# The webSite Job Type Name you want to create ‘triggered’ or ‘continuous’ Job
[Parameter(Mandatory = $true)]
[string]$JobType,

# The full path to copy files from.
[Parameter(Mandatory = $true)]
[string]$JobFile
)
Login-AzureRmAccount

# Ensure the local path given exists. Create it if switch specified to do so.
if (-not (Test-Path $JobFile))
{
throw “Source path ‘$LocalFilePath’ does not exist.  Specify an existing valid path.”
}

# To select specific subscription from available subscription “Visual Studio Enterprise with MSDN”
Select-AzureSubscription -SubscriptionName $AzureSubscriptionName
 

# Create the website
$website = Get-AzureWebsite | Where-Object {$_.Name -eq $WebSiteName }
if (!($website -eq $null))
{
    Write-Host “Creating website Job for ‘$WebSiteName’.”
    New-AzureWebsiteJob -Name $WebSiteName -JobName MyWebJob -JobType Continuous -JobFile $JobFile
    Write-Host “Website Job ‘$WebSiteJobName’ is created successfully.”
}
else
{
    throw “WebJob creation operation Failed. Website is not available.”
}