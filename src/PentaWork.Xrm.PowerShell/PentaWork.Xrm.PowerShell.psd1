@{
RootModule = 'PentaWork.Xrm.PowerShell.dll'
ModuleVersion = '1.9.1'
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
	'Update-XrmWebresource',
	'Set-XrmTimeout'
	)
PrivateData = @{
	PSData = @{
		Tags = @('XRM', 'Dynamics365', 'Dynamics')
		LicenseUri = 'https://github.com/pentawork-solutions/PentaWork.Xrm.PowerShell/LICENSE'
		ProjectUri = 'https://github.com/pentawork-solutions/PentaWork.Xrm.PowerShell'
		ReleaseNotes = @'
## 1.9.1
- Add formatted values to Fake classes

## 1.9.0
- Add 'ClearTargetRelations' for 'Import-XrmRelations' verb -> New properties for the exported json. If you want to use this property, please ensure to create a new export before.

## 1.8.1
- Add 'getEntityId' on form proxy

## 1.8.0
- Add Update-XrmWebresource

## 1.7.0
- Add removal of diacritics

## 1.6.7
- Fix a bug with multioptionset fields in Typescript

## 1.6.6
- Fix a bug while saving trough typescript

## 1.6.5
- Fix a bug with setting a proxy property to null in certain cases

## 1.6.4
- Fix a bug during export of money fields
- Update XRM nuget packages

## 1.6.3
- Fix a bug where multiple proxies for the same intersect entity were created

## 1.6.2
- Fix relation bug

## 1.6.1
- Fix logical name for intersect entities

## 1.6.0
- Add filter parameters for proxy generation (Solution, IncludeEntities, ExcludeEntities)

## 1.5.7
- Fix timeout issue for Cloud organizations

## 1.5.6
- Fix an error with null money values in the proxy classes

## 1.5.5
- Fix an exception in Get-XrmProxies

## 1.5.4
- Add rounding to money, decimal and double fields in proxy classes. Based on the configured field precision in CRM.
- Fix an error with false positive is dirty

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

