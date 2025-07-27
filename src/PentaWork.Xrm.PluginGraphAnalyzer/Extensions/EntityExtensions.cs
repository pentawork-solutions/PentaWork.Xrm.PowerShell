using Microsoft.Xrm.Sdk;

namespace PentaWork.Xrm.PluginGraph.Extensions
{
    internal static class EntityExtensions
    {
        public static T? AV<T>(this Entity entity, string alias)
            => entity.Contains(alias) && entity[alias] is AliasedValue av ? (T?)av.Value : default;
    }
}
