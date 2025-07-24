using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Creations
{
    internal class RequestCreationHook : ICreationHook
    {
        public void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters)
        {
            var apiCall = new XrmApiCall();
            apiCall.Message = "create";
            apiCall.IsExecuted = false;

            vmData.ApiCalls.Add(apiCall);
            vmData.Stack.Push(apiCall);
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            method.FullName == "System.Void Microsoft.Xrm.Sdk.Messages.CreateRequest::.ctor()";
    }
}
