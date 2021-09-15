@{
RootModule = 'PentaWork.Xrm.PowerShell.dll'
ModuleVersion = '1.1.0'
GUID = '3a2580f4-5ee8-41f6-b781-3893be7de311'
Author = 'Gerrit Gazic'
CompanyName = 'PentaWork Solutions GmbH & Co. KG'
Copyright = '(c) 2021 PentaWork Solutions GmbH & Co. KG. Alle Rechte vorbehalten.'
Description = 'PowerShell module for running tasks on Dynamics365/PowerApp/XRM'
PowerShellVersion = '5.0'
DotNetFrameworkVersion = '4.6.2'
RequiredModules = @('Microsoft.Xrm.Tooling.CrmConnector.PowerShell')
CmdletsToExport = @(
	'Export-XrmSolution',
	'Get-XrmProxies',
	'Get-XrmSolutions',
	'Import-XrmSolution',
	'Update-XrmAssembly'
	)
PrivateData = @{
	PSData = @{
		Tags = @('XRM', 'Dynamics365', 'Dynamics')
		LicenseUri = 'https://github.com/pentawork-solutions/PentaWork.Xrm.PowerShell/LICENSE'
		ProjectUri = 'https://github.com/pentawork-solutions/PentaWork.Xrm.PowerShell'
		ReleaseNotes = @'
## 1.1.0
- Add **Force** switch to **Export-XrmSolution**

## 1.0.2
- Initial Release
'@
		} 
	}
}

