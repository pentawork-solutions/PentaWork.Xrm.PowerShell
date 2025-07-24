using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Creations
{
    internal class OrganizationServiceContextCreationHook : ICreationHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var serviceContext = new ServiceContextObj();
            vmData.Stack.Push(serviceContext);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::.ctor(Microsoft.Xrm.Sdk.IOrganizationService)";
    }
}
