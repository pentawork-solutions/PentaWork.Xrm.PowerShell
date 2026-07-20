using System.Reflection;

namespace PentaWork.Xrm.PowerShell.Tests.Fixtures
{
    /// <summary>
    /// The CRM SDK metadata classes (EntityMetadata, AttributeMetadata, ...) are meant to be populated
    /// by deserializing a server response, so most collection-typed properties only expose an internal
    /// or private setter. Building a fixture by hand requires bypassing that via reflection.
    /// </summary>
    internal static class MetadataReflectionHelper
    {
        public static void SetProperty(object target, string propertyName, object? value)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new System.Exception($"Property '{propertyName}' not found on '{target.GetType()}'.");
            property.SetValue(target, value);
        }
    }
}
