using System;
using Microsoft.Xrm.Sdk.Metadata;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class AttributeInfo
    {
        public AttributeInfo(AttributeMetadata attrMetadata, string uniqueDisplayName)
        {
            LogicalName = attrMetadata.LogicalName;
            UniqueDisplayName = uniqueDisplayName;
            AttributeType = attrMetadata.AttributeType;
            AttributeMetadata = attrMetadata;
            ReturnType = GetReturnType(attrMetadata);
            JavascriptReturnType = GetJavascriptReturnType(attrMetadata);
            SetXrmTypingValues(attrMetadata);
        }

        public AttributeInfo(AttributeMetadata attrMetadata, string uniqueDisplayName, string returnType)
        {
            LogicalName = attrMetadata.LogicalName;
            UniqueDisplayName = uniqueDisplayName;
            AttributeType = attrMetadata.AttributeType;
            AttributeMetadata = attrMetadata;
            ReturnType = returnType;
            JavascriptReturnType = returnType;
            SetXrmTypingValues(attrMetadata);
        }

        private string GetReturnType(AttributeMetadata attrMetadata)
        {
            var returnType = string.Empty;
            switch(attrMetadata.AttributeType)
            {
                case AttributeTypeCode.PartyList:
                    returnType = "EntityCollection";
                    break;
                case AttributeTypeCode.Uniqueidentifier:
                    returnType = "Guid";
                    break;
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.EntityName:
                    returnType = "string";
                    break;
                case AttributeTypeCode.Decimal:
                    returnType = "decimal?";
                    break;
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                    returnType = "int?";
                    break;
                case AttributeTypeCode.Double:
                    returnType = "double?";
                    break;
                case AttributeTypeCode.Money:
                    returnType = "decimal?";
                    break;
                case AttributeTypeCode.Boolean:
                    returnType = "bool?";
                    break;
                case AttributeTypeCode.DateTime:
                    returnType = "DateTime?";
                    break;
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                    returnType = "EntityReference";
                    break;
                default:
                    throw new Exception($"{attrMetadata.LogicalName}");
            }
            return returnType;
        }

        private string GetJavascriptReturnType(AttributeMetadata attrMetadata)
        {
            var returnType = string.Empty;
            switch (attrMetadata.AttributeType)
            {
                case AttributeTypeCode.PartyList:
                    returnType = "Array<Xrm.LookupValue>";
                    break;
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.EntityName:
                    returnType = "string";
                    break;
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                case AttributeTypeCode.Decimal:
                    returnType = "number";
                    break;
                case AttributeTypeCode.Boolean:
                    returnType = "boolean";
                    break;
                case AttributeTypeCode.DateTime:
                    returnType = "Date";
                    break;
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Owner:
                    returnType = "Xrm.LookupValue";
                    break;
                case AttributeTypeCode.Lookup:
                    returnType = "Array<Xrm.LookupValue>";
                    break;
                default:
                    throw new Exception($"{attrMetadata.LogicalName}");
            }
            return returnType;
        }

        private void SetXrmTypingValues(AttributeMetadata attrMetadata)
        {
            switch (attrMetadata.AttributeType)
            {
                case AttributeTypeCode.Uniqueidentifier:
                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                case AttributeTypeCode.EntityName:
                    XrmTypingsAttributeType = "Xrm.Attributes.StringAttribute";
                    XrmTypingsControlType = "Xrm.Controls.StringControl";
                    break;
                case AttributeTypeCode.Money:
                case AttributeTypeCode.Double:
                case AttributeTypeCode.Integer:
                case AttributeTypeCode.BigInt:
                case AttributeTypeCode.Decimal:
                    XrmTypingsAttributeType = "Xrm.Attributes.NumberAttribute";
                    XrmTypingsControlType = "Xrm.Controls.NumberControl";
                    break;
                case AttributeTypeCode.Boolean:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                case AttributeTypeCode.Picklist:
                    XrmTypingsAttributeType = "Xrm.Attributes.OptionSetAttribute";
                    XrmTypingsControlType = "Xrm.Controls.OptionSetControl";
                    break;
                case AttributeTypeCode.DateTime:
                    XrmTypingsAttributeType = "Xrm.Attributes.DateAttribute";
                    XrmTypingsControlType = "Xrm.Controls.DateControl";
                    break;
                case AttributeTypeCode.PartyList:
                case AttributeTypeCode.Customer:
                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Owner:
                    XrmTypingsAttributeType = "Xrm.Attributes.LookupAttribute";
                    XrmTypingsControlType = "Xrm.Controls.LookupControl";
                    break;
                case AttributeTypeCode.Virtual:
                    XrmTypingsAttributeType = $"Xrm.Attributes.EnumAttribute<{JavascriptReturnType}>";
                    XrmTypingsControlType = "Xrm.Controls.OptionSetControl";
                    break;
                default:
                    throw new Exception($"{attrMetadata.LogicalName}");
            }
        }

        public string LogicalName { get; }
        public string UniqueDisplayName { get; }
        public string ReturnType { get; }

        public string XrmTypingsAttributeType { get; private set; }
        public string XrmTypingsControlType { get; private set; }
        public string JavascriptReturnType { get; private set; }

        public AttributeTypeCode? AttributeType { get; }
        public AttributeMetadata AttributeMetadata { get; }
    }
}
