using dnlib.DotNet;

namespace PentaWork.Xrm.PluginGraph.Model
{
    public class PluginModuleList : List<ModuleDefMD>
    {
        private readonly SigComparer _comparer = new();

        public new void Add(ModuleDefMD moduleDef)
        {
            if (this.SingleOrDefault(s => s.FullName == moduleDef.FullName) == null)
                base.Add(moduleDef);
        }

        public MethodDef? TryFindMethod(string declaringTypeFullName, IMethod method)
        {
            MethodDef? methodDef = null;
            foreach (var module in this)
            {
                methodDef = module.Types.SingleOrDefault(t => t.FullName == declaringTypeFullName)?.Methods.SingleOrDefault(f => f.Name == method.Name && _comparer.Equals(f.MethodSig, method.MethodSig));
                if (methodDef != null) break;
            }
            return methodDef;
        }
    }
}
