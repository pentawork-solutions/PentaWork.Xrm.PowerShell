using System;
using System.Management.Automation;
using Microsoft.Xrm.Tooling.Connector;

namespace PentaWork.Xrm.PowerShell.Verbs
{
    /// <summary>
    /// <para type="synopsis">Sets the timeout of the given XRM connection.</para>
    /// <para type="description">
    /// This function is able to set the connection timeout of an existing XRM connection.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>Get-CrmConnection -Interactive | Set-XrmTimeout -Timeout 30</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "XrmTimeout")]
    [OutputType(typeof(CrmServiceClient))]
    public class SetXrmTimeout : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            Connection.OrganizationServiceProxy.Timeout = new TimeSpan(0, Timeout, 0);
            WriteObject(Connection);
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
        /// <para type="description">The new timeout in minutes.</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public int Timeout { get; set; }
    }
}
