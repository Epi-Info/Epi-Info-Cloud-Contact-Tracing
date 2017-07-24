#Declare WebSiteName
 
param(
	# The webSite Name you want to create
	[Parameter(Mandatory = $true)]
	[string]$SqlDataBase,
 
    # The Server Instance
	[Parameter(Mandatory = $true)]
	[string]$SQlServerInstance, 

    # The Server Username
	[Parameter(Mandatory = $true)]
	[string]$SQLServerUsername, 

    # The ServerPassword
	[Parameter(Mandatory = $true)]
	[string]$SQLServerPassword,
  
    # The ServerPassword
	[Parameter(Mandatory = $true)]
	[string]$SQLScriptPath
	 
)

#$SqlDataBase ="EICDCPRO"# $sheet.Cells.Item(37,3).Value()
#$SQlServerInstance ="tcp:eicsraDEV.database.windows.net,1433" #$sheet.Cells.Item(38,3).Value()
#$SQLServerUsername     ="eicsraadmin" #$sheet.Cells.Item(39,3).Value()
#$SQLServerPassword     ="hK=JDZNjW8!pv" #$sheet.Cells.Item(40,3).Value() 

$params = @{

  'Database' = $SqlDataBase

  'ServerInstance' =  $SQlServerInstance

  'Username' = $SQLServerUsername

  'Password' = $SQLServerPassword

  'OutputSqlErrors' = $true

  }
 

Try
{
    # Start the deployment
    Write-Host "Starting SQL ...";   
 
    Add-PSSnapin SqlServerCmdletSnapin120
    Add-PSSnapin SqlServerProviderSnapin120

    #"C:\Users\Desktop\SQLScript\EICDCSQlScript.sql"
    Invoke-Sqlcmd -inputfile $SQLScriptPath @params
 
    #Ouput File
    $ScriptPath = $MyInvocation.MyCommand.Path
    $strUser="Epi Info Cloud Data catpure Azure Steup Completed Successfully"+$SqlDataBase
    $OutFileName = "$($ScriptPath.SubString(0, $ScriptPath.Length - 4))_InserQueryResult.txt" 
    $strUser | Out-File $OutFileName  

     Write-Host "Finished SQL ..."; 
}
Catch
{
    Write-Host $_.Exception.Message -ForegroundColor Yellow      
}