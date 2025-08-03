using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks
{
    internal interface IHook
    {
        bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters);
        XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, ref Stack<object> stack);
    }
}
