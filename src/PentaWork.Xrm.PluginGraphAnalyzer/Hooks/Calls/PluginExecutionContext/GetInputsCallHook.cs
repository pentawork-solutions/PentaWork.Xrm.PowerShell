using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls.PluginExecutionContext
{
    internal class GetInputsCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            storageFrame.Stack.Push(parameters[0]);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            (method.FullName is "Microsoft.Xrm.Sdk.ParameterCollection Microsoft.Xrm.Sdk.IExecutionContext::get_InputParameters()"
            or "Microsoft.Xrm.Sdk.EntityImageCollection Microsoft.Xrm.Sdk.IExecutionContext::get_PreEntityImages()")
            && parameters.Count > 0
            && parameters[0] is ExecutionContextObj;
    }
}
