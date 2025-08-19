using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;

namespace PentaWork.Xrm.PluginGraph.Hooks
{
    internal interface IHook
    {
        bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame);
        void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame);
    }
}
