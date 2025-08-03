namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class GenericObj : IVMObj
    {
        public GenericObj(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Converts the Generic Object to a "real" one, which we are able to use in the call hooks
        /// to keep track of entity manipulations and service api calls.
        /// </summary>
        /// <returns></returns>
        public object GetObject()
        {
            if (Fields.ContainsKey("System.String Microsoft.Xrm.Sdk.Entity::_logicalName"))
            {
                var entity = new EntityObj
                {
                    LogicalName = (string)Fields["System.String Microsoft.Xrm.Sdk.Entity::_logicalName"]
                };
                return entity;
            }
            if (Fields.ContainsKey("System.String Microsoft.Xrm.Sdk.OrganizationRequest::_messageName"))
            {
                var apiCall = new XrmApiCall
                {
                    Message = (string)Fields["System.String Microsoft.Xrm.Sdk.OrganizationRequest::_messageName"],
                    EntityInfo = Fields.ContainsKey("Target") ? (EntityObj)Fields["Target"] : null,
                    IsExecuted = false
                };
                return apiCall;
            }
            if (Fields.ContainsKey("Microsoft.Xrm.Sdk.IOrganizationService Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::_service")
                || Name == "New Object (System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::.ctor(Microsoft.Xrm.Sdk.IOrganizationService))")
            {
                return new ServiceContextObj();
            }
            return this;
        }

        public string Name { get; }
        public Dictionary<string, object> Fields { get; } = new Dictionary<string, object>();
    }
}
