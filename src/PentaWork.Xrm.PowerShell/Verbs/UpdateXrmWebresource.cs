using System;
using System.IO;
using System.Management.Automation;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PowerShell.Common;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Updates an existing webresource in the target system.</para>
    /// <para type="description">
    /// This function updates a given webresource in the target system.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>$conn = Get-CrmConnection -InteractiveMode</para>
    /// <para>Update-XrmWebresource -Connection $conn -WebresourceFile .\webresource.js -Name pw_/javascripts/webresource.js</para>
    /// </example>
    [Cmdlet(VerbsData.Update, "XrmWebresource")]
    public class UpdateXrmWebresource : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteVerbose($"Processing '{WebresourceFile.Name}' ...");

            var webresourceQuery = new QueryExpression
            {
                EntityName = "webresource",
                ColumnSet = new ColumnSet(true)
            };
            webresourceQuery.Criteria.AddCondition("name", ConditionOperator.Equal, Name);

            var response = Connection.Query(webresourceQuery, true);
            if (response != null)
            {
                if (response.Count > 1) { WriteWarning($"More than one webresource with name '{Name}' found! Skipping!"); }
                else
                {
                    var crmWebresource = response[0];
                    crmWebresource["content"] = Convert.ToBase64String(File.ReadAllBytes(WebresourceFile.FullName));

                    WriteVerbose($"Updating webresource '{Name}' to XRM ...");
                    Connection.Update(crmWebresource);

                    if(Publish)
                    {
                        WriteVerbose($"Publishing webresource ...");
                        Connection.Execute(new PublishXmlRequest
                        {
                            ParameterXml = $"<importexportxml><webresources><webresource>{crmWebresource.Id}</webresource></webresources></importexportxml>"
                        });
                    }
                    WriteVerbose($"Updating done!");
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
        /// <para type="description">The path to the webresource to update in the XRM system.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public FileInfo WebresourceFile { get; set; }

        /// <summary>
        /// <para type="description">The logical name of the webresource to update in the XRM system.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }


        /// <summary>
        /// <para type="description">Publish the web resource after update.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Publish { get; set; }
    }
}
