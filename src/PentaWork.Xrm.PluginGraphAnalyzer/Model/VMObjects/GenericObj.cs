using dnlib.DotNet;

namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class GenericObj : IVMObj
    {
        public GenericObj(string name, ITypeDefOrRef typeDefOrRef, bool isRecursiveReturnValue = false)
        {
            Name = name;
            TypeDefOrRef = typeDefOrRef;
            IsRecursiveReturnValue = isRecursiveReturnValue;
        }

        /// <summary>
        /// Converts the Generic Object to a "real" one, which we are able to use in the call hooks
        /// to keep track of entity manipulations and service api calls.
        /// </summary>
        /// <returns></returns>
        public object GetObject()
        {
            if (Fields.ContainsKey("EntityObj.LogicalName"))
            {
                var entity = new EntityObj
                {
                    LogicalName = (string)Fields["EntityObj.LogicalName"]
                };
                return entity;
            }
            if (Fields.ContainsKey("OrganizationServiceContext.Service"))
            {
                return new ServiceContextObj();
            }
            if (Fields.ContainsKey("OrganizationRequest.MessageName"))
            {
                var apiCall = new XrmApiCall
                {
                    Message = (string)Fields["OrganizationRequest.MessageName"],
                    EntityInfo = Fields.ContainsKey("OrganizationRequest.Target") ? (EntityObj)Fields["OrganizationRequest.Target"] : null,
                    IsExecuted = false
                };
                return apiCall;
            }
            return this;
        }

        public string Name { get; }
        public ITypeDefOrRef TypeDefOrRef { get; }
        public bool IsRecursiveReturnValue { get; } = false;
        public List<object> Parameters { get; set; }
        public Dictionary<string, object> Fields { get; } = new Dictionary<string, object>();
    }
}
