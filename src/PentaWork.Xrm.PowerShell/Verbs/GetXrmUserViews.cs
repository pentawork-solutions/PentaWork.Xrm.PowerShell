using System;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PowerShell.Common;
using Microsoft.Xrm.Sdk.Query;

namespace PentaWork.Xrm.PowerShell
{
    #region DTOs
    /// <summary>
    /// <para type="synopsis">Object to hold view, dashboard and chart information.</para>
    /// </summary>
    public class UserObjectInfo
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public Guid ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
    }
    #endregion

    /// <summary>
    /// <para type="synopsis">Gets a list of all user views, dashboards and charts.</para>
    /// <para type="description">
    /// The function returns a list of user views, dashboards and charts.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>Get-CrmConnection -InteractiveMode | Get-XrmUserViews</code>
    /// </example>
    [OutputType(typeof(List<UserObjectInfo>))]
    [Cmdlet(VerbsCommon.Get, "XrmUserViews")]
    public class GetXrmUserViews : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteProgress(new ProgressRecord(0, "Retrieving", "Retrieving all users ...") { PercentComplete = 0 });

            var query = new QueryExpression
            {
                EntityName = "systemuser",
                ColumnSet = new ColumnSet("fullname")
            };
            query.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            var users = Connection.Query(query, true);

            var processed = 0;
            var userObjects = new List<UserObjectInfo>();
            foreach(var user in users)
            {
                WriteProgress(new ProgressRecord(0, "Retrieving", $"Retrieving views for user '{user["fullname"]}' ...") { PercentComplete = 20 + (80 * processed++ / users.Count) });
                userObjects.AddRange(GetUserObjects(user));
            }
            WriteProgress(new ProgressRecord(0, "Retrieving", "Done!") { RecordType = ProgressRecordType.Completed });
            WriteObject(userObjects);
        }

        private List<UserObjectInfo> GetUserObjects(Entity user)
        {
            var originalCaller = Connection.CallerId;
            Connection.CallerId = user.Id;

            var views = Connection.GetAll("userquery", "name");
            var charts = Connection.GetAll("userqueryvisualization", "name");
            var dashboards = Connection.GetAll("userform", "name");

            Connection.CallerId = originalCaller;
            return views.Select(v => new UserObjectInfo { UserId = user.Id, FullName = user["fullname"].ToString(), ObjectType = "View", ObjectId = v.Id, ObjectName = v["name"].ToString() })
                .Concat(charts.Select(c => new UserObjectInfo { UserId = user.Id, FullName = user["fullname"].ToString(), ObjectType = "Chart", ObjectId = c.Id, ObjectName = c["name"].ToString() }))
                .Concat(dashboards.Select(d => new UserObjectInfo { UserId = user.Id, FullName = user["fullname"].ToString(), ObjectType = "Dashboard", ObjectId = d.Id, ObjectName = d["name"].ToString() }))
                .ToList();
        }

        /// <summary>
        /// <para type="description">The connection to the XRM Organization (Get-CrmConnection).</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public CrmServiceClient Connection { get; set; }
    }
}
