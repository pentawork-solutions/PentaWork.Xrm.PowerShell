using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Updates a plugin assembly in the target system.</para>
    /// <para type="description">
    /// This function updates a given plugin assembly in the target system.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -Interactive</para>
    /// <para>Update-XrmAseembly -Connection $conn -AssemblyFile .\Plugin.dll</para>
    /// </example>
    [Cmdlet(VerbsData.Update, "XrmAssembly")]
    public class UpdateXrmAssembly : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(AssemblyFile.FullName);
            var assemblyProperties = assembly.GetName().FullName.Split(",= ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var assemblyShortName = assemblyProperties[0];

            Console.WriteLine($"Processing assembly '{assemblyShortName}' ...");

            var assemblyQuery = new QueryExpression
            {
                EntityName = "pluginassembly",
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression()
            };
            assemblyQuery.Criteria.AddCondition("name", ConditionOperator.Equal, assemblyShortName);

            var response = Connection.RetrieveMultiple(assemblyQuery).Entities;
            if (response != null)
            {
                if (response.Count > 1) { WriteWarning($"More than one assembly with name '{assemblyShortName}' found! Skipping!"); }
                else
                {
                    var crmAssembly = response[0];
                    crmAssembly["version"] = assemblyProperties[2];
                    crmAssembly["culture"] = assemblyProperties[4];
                    crmAssembly["publickeytoken"] = assemblyProperties[6];
                    crmAssembly["content"] = Convert.ToBase64String(File.ReadAllBytes(assembly.Location));

                    Console.WriteLine($"Publishing assembly '{assemblyShortName}' to XRM ...");
                    Connection.Update(crmAssembly);
                    Console.WriteLine($"Publishing done!");
                }
            }
        }

        /// <summary>
        /// <para type="description">The connection to the XRM Organization (Get-CrmConnection)</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public CrmServiceClient Connection { get; set; }

        /// <summary>
        /// <para type="description">The path to the assembly to update in the XRM system.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public FileInfo AssemblyFile { get; set; }
    }
}
