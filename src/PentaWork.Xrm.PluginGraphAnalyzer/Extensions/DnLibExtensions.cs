using dnlib.DotNet;

namespace PentaWork.Xrm.PluginGraph.Extensions
{
    internal static class DnLibExtensions
    {
        public static TypeDef? ResolveTypeDef(this TypeSig typeSig, IMethod methodContext, ITypeDefOrRef? typeContext)
        {
            TypeDef? typeDef = null;

            var genVar = GetGenericArgument(methodContext);
            if (typeSig.IsGenericInstanceType) typeDef = typeSig.ToTypeDefOrRef().ResolveTypeDef();
            else if (genVar is ClassSig classSig) typeDef = classSig.TypeDefOrRef.ResolveTypeDef();
            else if (typeSig.IsGenericMethodParameter && genVar is GenericMVar genMVar)
            {
                var callingMethod = methodContext as MethodSpec;
                var genericMethodSig = callingMethod?.GenericInstMethodSig;

                if (genericMethodSig != null)
                    typeDef = GetGenericTypeDef(genericMethodSig.GenericArguments, (int)genMVar.Number);
            }
            else if (typeSig.IsGenericTypeParameter && typeContext != null && genVar is GenericVar genTVar)
            {
                var baseTypeDefOrRef = typeContext.ResolveTypeDef().GetBaseType();
                var baseTypeSig = baseTypeDefOrRef.ToTypeSig();

                // If the generic method argument references an argument of the generic class, the type signature of the containing type, has to be a generic signature
                if (genVar != null && typeContext.ResolveTypeDef().ToTypeSig() is GenericInstSig genericTypeSig)
                    typeDef = GetGenericTypeDef(genericTypeSig.GenericArguments, (int)genTVar.Number);
                else if (genVar != null && baseTypeSig != null && baseTypeSig is GenericInstSig genericBaseTypeSig)
                    typeDef = GetGenericTypeDef(genericBaseTypeSig.GenericArguments, (int)genTVar.Number);
            }

            return typeDef;
        }

        private static TypeSig? GetGenericArgument(IMethod method) => ((method as MethodSpec)?.Instantiation as GenericInstMethodSig)?.GenericArguments.FirstOrDefault();

        private static TypeDef? GetGenericTypeDef(IList<TypeSig> genericTypes, int varNr)
        {
            TypeDef? typeDef = null;

            var genericArg = genericTypes[varNr];
            var genericDefOrRef = genericArg.ToTypeDefOrRef();
            if (genericDefOrRef is TypeRef typeRef) typeDef = typeRef.Resolve();
            else typeDef = genericDefOrRef as TypeDef;

            return typeDef;
        }
    }
}
