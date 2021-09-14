using Microsoft.Xrm.Sdk.Metadata;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class OptionInfo
    {
        public OptionInfo(OptionMetadata optionMetadata, string uniqueValueName)
        {
            DisplayName = optionMetadata.Label.GetLabel();
            UniqueValueName = uniqueValueName;
            Value = optionMetadata.Value.Value;
            Description = optionMetadata.Description?.GetLabel() ?? string.Empty;
        }

        public string DisplayName { get; }
        public string UniqueValueName { get; }
        public string Description { get; }
        public int Value { get; }
    }
}
