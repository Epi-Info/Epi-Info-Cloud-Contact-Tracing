
#Change the Template File Path and Parameters File Path

$TemplateFileName="Template.json"
$ParameterFileName="Parameters.json"


$ScriptPathName="PowershellRelease1.0"
$FileName="EpiCloudDeploymentExcel.xlsm"

$RootPath = Split-Path (Split-Path $PSScriptRoot -Parent)
$WebSitePackagePath =$RootPath + "\" + $ScriptPathName + "\" + "EpiCloud.zip"
$WebApiPackagePath =$RootPath + "\" + $ScriptPathName + "\" + "MetaDataAccessApi.zip"
$WebJobPackagePath =$RootPath + "\" + $ScriptPathName + "\" + "EpiWebjob\bin\Release\Epi.Cloud.WebJobs.exe"
$templateFilePath= $RootPath + "\" + $ScriptPathName + "\" + $TemplateFileName
$parametersFilePath= $RootPath + "\" + $ScriptPathName + "\" + $ParameterFileName

<#Set the output path file #>
$RootPath = Split-Path (Split-Path $PSScriptRoot -Parent)
$CurrentFilePath= $RootPath + "\" + $ScriptPathName + "\" + $FileName
$ScriptPath = $MyInvocation.MyCommand.Path
$OutPutFileName = "$($ScriptPath.SubString(0, $ScriptPath.Length - 4))_Results.txt"


#Declare the file path and sheet name
$file =$RootPath + "\" + $ScriptPathName + "\" + $FileName
$sheetName = "EpiCloudSetup"

#Create an instance of Excel.Application and Open Excel file
$objExcel = New-Object -ComObject Excel.Application
$workbook = $objExcel.Workbooks.Open($file)
$sheet = $workbook.Worksheets.Item($sheetName)


$SubscriptionId        = $sheet.Cells.Range("subscriptionId").Value()
$SubscriptionName      = $sheet.Cells.Range("subscriptionName").Value()
$ResourceGroupName     = $sheet.Cells.Range("resourceGroupName").Value()
$ResourceGroupLocation = $sheet.Cells.Range("resourceLocation").Value()
$WebSiteName =  $sheet.Cells.Item(15,3).Value() 
$WebApiName =  $sheet.Cells.Item(16,3).Value()
$DataConsistencyApiName =  $sheet.Cells.Item(17,3).Value() 
$JobName =  "GetDataFromServiceBusToSQLJob"
$WebJobType=  "continuous" 



<############################################>
    <#Write Script result to file#>
    Function Script-Output {
    [cmdletbinding()]
    Param ([string]$FilePath,[string]$Message) 
     $Message | Out-File $FilePath
    }
<############################################>


<############################################>
<# Publish WebApp #>
    Function Publish-WebAppandApi{
    [cmdletbinding()]
    Param ([string]$WebPackagePath,[string]$WebName) 
        try {
              $WebStatus=Get-AzureWebsite -Name $WebName
              if($WebStatus.State -eq "Running"){
                 Publish-AzureWebsiteProject -Package $WebPackagePath -Name $WebName
                 $SuccessMessage="Publish $WebName Success" 
                 Script-Output -FilePath $OutPutFileName -Message $SuccessMessage
              } 
            else{             
                 $ErrorMessage="Web App $WebName is not running " 
                 Script-Output -FilePath $OutPutFileName -Message $ErrorMessage 
            }        
        }
        catch {
              #Ouput File 
             $ErrorMessage="Publish +$WebName+ Failed" 
             Script-Output -FilePath $OutPutFileName -Message $ErrorMessage 
        }
    }
<############################################> 

<############################################>
<# Publish Web Job#>
    Function Publish-WebJob{
    [cmdletbinding()]
    Param ([string]$WebName,[string]$WebJobName,[string]$WebJobType,[string]$WebJobPackagePath) 
        try {
              $WebStatus=Get-AzureWebsite -Name $WebName
              if($WebStatus.State -eq "Running"){
                 New-AzureWebsiteJob -Name $WebName -JobName $WebJobName -JobType $WebJobType -JobFile $WebJobPackagePath
                 $SuccessMessage="Publish Web Job $WebName Success" 
                 Script-Output -FilePath $OutPutFileName -Message $SuccessMessage
              } 
            else{             
                 $ErrorMessage="Publish Web Job $WebName is not running" 
                 Script-Output -FilePath $OutPutFileName -Message $ErrorMessage 
            }        
        }
        catch {
              #Ouput File 
             $ErrorMessage="Publish +$WebName+ Failed" 
             Script-Output -FilePath $OutPutFileName -Message $ErrorMessage 
        }
    }
<############################################>

#close excel file
$objExcel.quit()
<#
.SYNOPSIS
    Registers RPs
#>
Function RegisterRP {
    Param(
        [string]$ResourceProviderNamespace
    )

    Write-Host "Registering resource provider '$ResourceProviderNamespace'";
    Register-AzureRmResourceProvider -ProviderNamespace $ResourceProviderNamespace;
}

#******************************************************************************
# Script body
# Execution begins here
#******************************************************************************
#$ErrorActionPreference = "Stop"

# sign in
Write-Host "Logging in...";
Login-AzureRmAccount;


# select subscription
Write-Host "Selecting subscription '$SubscriptionId'";
#Select-AzureRmSubscription -SubscriptionID $SubscriptionId;

# Register RPs
$resourceProviders = @("microsoft.cache","microsoft.documentdb","microsoft.insights","microsoft.servicebus","microsoft.sql","microsoft.storage","microsoft.web");
if($resourceProviders.length) {
    Write-Host "Registering resource providers"
    foreach($resourceProvider in $resourceProviders) {
        RegisterRP($resourceProvider);
    }
}

#Create or check for existing resource group
$resourceGroup = Get-AzureRmResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
if(!$resourceGroup)
{
    Write-Host "Resource group '$ResourceGroupName' does not exist. To create a new resource group, please enter a location.";
    if(!$ResourceGroupLocation) {
        $ResourceGroupLocation = Read-Host "ResourceGroupLocation";
    }
    Write-Host "Creating resource group '$ResourceGroupName' in location '$ResourceGroupLocation'";
    New-AzureRmResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation;
    echo "Sucess fully Complted"
}
else{
    Write-Host "Using existing resource group '$ResourceGroupName'";
}

Try
{

    # Start the deployment
    Write-Host "Starting deployment...";
    if(Test-Path $parametersFilePath) {
        New-AzureRmResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile $templateFilePath -TemplateParameterFile $parametersFilePath;
    } else {
        New-AzureRmResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile $templateFilePath;
    }

    #Ouput File 
     $SuccessMessage="Epi Cloud Data Capture create resource Success.Go to azure portal check the created resources" 
     Script-Output -FilePath $OutPutFileName -Message $SuccessMessage 
}
Catch
{
    Write-Host $_.Exception.Message -ForegroundColor Yellow

    Write-Host "Removing the resource group '$ResourceGroupName'";

    #Ouput File 
    $ErrorMessage="Epi Cloud Data Capture create resource fail.All create resources are deleted" 
    Script-Output -FilePath $OutPutFileName -Message $ErrorMessage 
    #Remove-AzureRmResourceGroup $ResourceGroupName -force    
}




<# Publish Web App #>
Publish-WebAppandApi -WebPackagePath $WebSitePackagePath -WebName $WebSiteName

<# Publish Web API #>
Publish-WebAppandApi -WebPackagePath $WebApiPackagePath -WebName $WebApiName 

<# Publish Web Job#>
Publish-WebJob -WebName $DataConsistencyApiName -WebJobName $JobName -WebJobType $WebJobType -WebJobPackagePath $WebJobPackagePath 