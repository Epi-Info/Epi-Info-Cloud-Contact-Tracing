#Declare WebSiteName

#Login-AzureRmAccount

$ScriptPathName="PowershellRelease1.0"
$FileName="EpiCloudDeploymentExcel.xlsm"


$RootPath = Split-Path (Split-Path $PSScriptRoot -Parent) 
$ScriptPath = $MyInvocation.MyCommand.Path

 
#Declare the file path and sheet name 
$file =$RootPath + "\" + $ScriptPathName + "\" + $FileName
$OutPutFileName = "$($ScriptPath.SubString(0, $ScriptPath.Length - 4))_Results.txt"
 
$sheetName = "EpiCloudSetup"

#Create an instance of Excel.Application and Open Excel file
$objExcel = New-Object -ComObject Excel.Application
$workbook = $objExcel.Workbooks.Open($file)
$sheet = $workbook.Worksheets.Item($sheetName)
$objExcel.Visible=$false 
 

#Get the WebApp and Api Name   
$WebSite=$sheet.Cells.Item(15,3).Value() 
$MetaDataApi=$sheet.Cells.Item(16,3).Value()
$DataConsistencyWebJob= $sheet.Cells.Item(17,3).Value()


#Get the Existing Connection String Keys and Values  WebApp and Api Name 
$WebAppSettings = (Get-AzureWebsite $WebSite).AppSettings
#$MetaApiSettings = (Get-AzureWebsite $MetaDataApi).AppSettings
#$WebJobApi = (Get-AzureWebsite $DataConsistencyWebJob).AppSettings

#loop to get values and store it
for($i=34; $i -le 36; $i++)
{
    $WebAppSettings.Add($sheet.Cells.Item($i,2).text,$sheet.Cells.Item($i,3).text) 
	#$MetaApiSettings.Add($sheet.Cells.Item($i,2).text,$sheet.Cells.Item($i,3).text) 
	#$WebJobApi.Add($sheet.Cells.Item($i,2).text,$sheet.Cells.Item($i,3).text) 
} 

#close excel file
$objExcel.quit() 


<############################################>
    <#Write Script result to file#>
    Function Script-Output {
    [cmdletbinding()]
    Param ([string]$FilePath,[string]$Message) 
     $Message | Out-File $FilePath
    }
<############################################>

Try
{

    # Start the deployment
    Write-Host "Update App Settings..EICDCApp";    
    Set-AzureWebsite -Name $WebSite -AppSettings $WebAppSettings 
	
	Write-Host "Update App Settings..MetaDataApi";    
    Set-AzureWebsite -Name $MetaDataApi -AppSettings $WebAppSettings 
	
	Write-Host "Update App Settings..DataConsistencyService WebJob";    
    Set-AzureWebsite -Name $DataConsistencyWebJob -AppSettings $WebAppSettings 

    #Ouput File 
     $SuccessMessage="Epi Cloud Data Capture Web App and API Updated App Settings Success" 
     Script-Output -FilePath $OutPutFileName -Message $SuccessMessage 
}
Catch
{
    Write-Host $_.Exception.Message -ForegroundColor Yellow 

    #Ouput File 
    $ErrorMessage="Epi Cloud Data Capture Web App Updated App Settings Fail" 
    Script-Output -FilePath $OutPutFileName -Message $ErrorMessage  
}