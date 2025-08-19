using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Hooks.Calls
{
    internal class ActivatorCallHook : IHook
    {
        public void ExecuteHook(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame)
        {
            var typeDef = GetDirectGeneric(method) ?? (GetMethodGeneric(method, storageFrame) ?? GetClassGeneric(method, storageFrame));
            var ctorMethod = typeDef.Methods.Single(m => m.FullName.EndsWith(".ctor()"));
            var genericObj = new GenericObj("Generic Activator Object", method.DeclaringType);
            var vm = new PluginGraphVM(new PluginModuleList(), new List<string>()); // Just analyze the ctor to get the entity name - we dont need any modules or sub namespaces we want to analyze
            vm.Execute(ctorMethod, null, new List<object> { genericObj });

            storageFrame.Stack.Push(genericObj.GetObject());
        }

        public bool HookApplicable(IMethod method, MethodDef? methodDef, List<object> parameters, StorageFrame storageFrame) =>
            method.FullName.Contains("System.Activator::CreateInstance")
            && (GetDirectGeneric(method) != null || GetMethodGeneric(method, storageFrame) != null || GetClassGeneric(method, storageFrame) != null);

        private TypeSig? GetGenericArgument(IMethod method) => ((method as MethodSpec)?.Instantiation as GenericInstMethodSig)?.GenericArguments.FirstOrDefault();

        private TypeDef? GetGenericTypeDef(IList<TypeSig> genericTypes, int varNr)
        {
            TypeDef? typeDef = null;

            var genericArg = genericTypes[varNr];
            var genericDefOrRef = genericArg.ToTypeDefOrRef();
            if (genericDefOrRef is TypeRef typeRef) typeDef = typeRef.Resolve();
            else typeDef = genericDefOrRef as TypeDef;

            return typeDef;
        }

        // The Activator uses a direct type def: Activator.CreateInstance<Account>()
        private TypeDef? GetDirectGeneric(IMethod method)
        {
            TypeDef? typeDef = null;
            var genTypeSig = GetGenericArgument(method) as TypeDefOrRefSig;
            if (genTypeSig?.TypeDefOrRef is TypeRef typeRef) typeDef = typeRef.Resolve();
            else typeDef = genTypeSig?.TypeDef;

            return typeDef;
        }

        private TypeDef? GetMethodGeneric(IMethod method, StorageFrame storageFrame)
        {
            TypeDef? typeDef = null;
            var genVar = GetGenericArgument(method) as GenericMVar;
            var callingMethod = storageFrame.Method as MethodSpec;
            var genericMethodSig = callingMethod?.GenericInstMethodSig;

            if (genVar != null && genericMethodSig != null)
                typeDef = GetGenericTypeDef(genericMethodSig.GenericArguments, (int)genVar.Number);

            return typeDef;
        }

        // The Activator uses a generic parameter of the class which contains the method which calls the Activator
        // class Test<T> {
        //    public void Test(){
        //      Activator.CreateInstance<T>();
        //    }
        // }
        private TypeDef? GetClassGeneric(IMethod method, StorageFrame storageFrame)
        {
            TypeDef? typeDef = null;
            var genVar = GetGenericArgument(method) as GenericVar;
            var typeInstance = storageFrame.Parameters.FirstOrDefault() as GenericObj;

            var baseTypeDefOrRef = typeInstance?.TypeDefOrRef.GetBaseType();
            var baseTypeSig = baseTypeDefOrRef.ToTypeSig();

            // If the generic method argument references an argument of the generic class, the type signature of the containing type, has to be a generic signature
            if (genVar != null && typeInstance != null && typeInstance.TypeDefOrRef.ToTypeSig() is GenericInstSig genericTypeSig)
                typeDef = GetGenericTypeDef(genericTypeSig.GenericArguments, (int)genVar.Number);
            else if (genVar != null && baseTypeSig != null && baseTypeSig is GenericInstSig genericBaseTypeSig)
                typeDef = GetGenericTypeDef(genericBaseTypeSig.GenericArguments, (int)genVar.Number);

            return typeDef;
        }
    }
}
