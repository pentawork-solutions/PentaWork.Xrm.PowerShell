using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class SetAttributeCallHook : ICallHook
    {
        public XrmApiCall? ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, ref Stack<object> stack)
        {
            var entity = (EntityObj)parameters[0];
            entity.UsedFields.Add((string)parameters[1]);

            return null;
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters) =>
            parameters.Count > 0 && parameters[0] is EntityObj && method.FullName is
                "System.Void Microsoft.Xrm.Sdk.Entity::SetAttributeValue(System.String,System.Object)" or
                "System.Void Microsoft.Xrm.Sdk.DataCollection`2<System.String,System.Object>::set_Item(System.String,System.Object)";
    }
}
