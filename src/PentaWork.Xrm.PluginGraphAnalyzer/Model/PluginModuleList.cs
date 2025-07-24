using dnlib.DotNet;

namespace PentaWork.Xrm.PluginGraph.Model
{
    public class PluginModuleList : List<ModuleDefMD>
    {
        public MethodDef? TryFindMethod(string declaringTypeFullName, string methodFullName)
        {
            MethodDef? methodDef = null;
            foreach (var module in this)
            {
                methodDef = module.Types.SingleOrDefault(t => t.FullName == declaringTypeFullName)?.Methods.SingleOrDefault(f => f.FullName == methodFullName);
                if (methodDef != null) break;
            }
            return methodDef;
        }
    }
}
