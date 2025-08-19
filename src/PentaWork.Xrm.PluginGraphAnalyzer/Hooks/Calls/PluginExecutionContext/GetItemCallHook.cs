using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class GetInputParametersCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var executionContext = (ExecutionContextObj)parameters[0];
            storageFrame.Stack.Push(executionContext.GetItem((string)parameters[1]));
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            parameters.Count > 0
            && parameters[0] is ExecutionContextObj
            && (method.FullName is "System.Object Microsoft.Xrm.Sdk.DataCollection`2<System.String,System.Object>::get_Item(System.String)"
            or "Microsoft.Xrm.Sdk.Entity Microsoft.Xrm.Sdk.DataCollection`2<System.String,Microsoft.Xrm.Sdk.Entity>::get_Item(System.String)");
    }
}
