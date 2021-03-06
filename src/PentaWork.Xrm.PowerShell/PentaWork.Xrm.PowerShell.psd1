@{
RootModule = 'PentaWork.Xrm.PowerShell.dll'
ModuleVersion = '1.5.3'
GUID = '3a2580f4-5ee8-41f6-b781-3893be7de311'
Author = 'Gerrit Gazic'
CompanyName = 'PentaWork Solutions GmbH & Co. KG'
Copyright = '(c) 2021 PentaWork Solutions GmbH & Co. KG. Alle Rechte vorbehalten.'
Description = 'PowerShell module for running tasks on Dynamics365/PowerApp/XRM'
PowerShellVersion = '5.0'
DotNetFrameworkVersion = '4.6.2'
RequiredModules = @('Microsoft.Xrm.Tooling.CrmConnector.PowerShell')
CmdletsToExport = @(
	'Remove-XrmEntities',
	'Export-XrmEntities',
	'Export-XrmSolution',
	'Get-XrmProxies',
	'Get-XrmSolutions',
	'Get-XrmUserViews',
	'Import-XrmSolution',
	'Import-XrmSharings',
	'Import-XrmEntities',
	'Import-XrmRelations',
	'Update-XrmAssembly',
	'Set-XrmTimeout'
	)
PrivateData = @{
	PSData = @{
		Tags = @('XRM', 'Dynamics365', 'Dynamics')
		LicenseUri = 'https://github.com/pentawork-solutions/PentaWork.Xrm.PowerShell/LICENSE'
		ProjectUri = 'https://github.com/pentawork-solutions/PentaWork.Xrm.PowerShell'
		ReleaseNotes = @'
## 1.5.3
- Fixed relation import for missing relation conditions

## 1.5.2
- Fix an error with the new typecodes

## 1.5.1
- Sort Exported Entities in Export-XrmEntities
- Add Typecode to Proxyclasses in Get-XrmProxies

## 1.5.0
- Add optional base proxy class to get track changes on an entity (Get-XrmProxies)
- Add Export-XrmEntites verb
- Add Get-XrmUserViews verb
- Add Import-XrmEntities verb
- Add Import-XrmSharings verb
- Add Import-XrmRelations verb
- Add Remove-XrmEntities verb
- Switch to native powershell output (Verbose, Warnings & Progress)

## 1.4.2
- Fix a multi select option sets bug

## 1.4
- Add action wrappers to entities

## 1.3.2
- Sort Entity Members to prevent false updates

## 1.3.1
- Add change detection for proxy attributes

## 1.3.0
- Remove the creation of private Entities and Entities without any public SDK Message
- Fixed the ID Attribute generation for proxy classes

## 1.2.2
- Fixed a bug with the output path for the Get-XrmProxies verb

## 1.2.0
- Add **Set-XrmTimeout**
- Add 'unmanaged' to the file name of unmanaged exported solutions

## 1.1.0
- Add **Force** switch to **Export-XrmSolution**

## 1.0.2
- Initial Release
'@
		} 
	}
}

