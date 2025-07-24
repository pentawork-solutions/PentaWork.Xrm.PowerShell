using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;

namespace PentaWork.Xrm.PluginGraph.Hooks
{
    internal interface IHook
    {
        bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters);
        void ExecuteHook(PluginGraphVMData vmData, IMethod method, MethodDef? methodDef, List<object> parameters);
    }
}
