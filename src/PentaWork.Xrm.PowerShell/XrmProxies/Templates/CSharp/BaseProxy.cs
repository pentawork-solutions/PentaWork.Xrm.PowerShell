﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion: 17.0.0.0
//  
//     Änderungen an dieser Datei können fehlerhaftes Verhalten verursachen und gehen verloren, wenn
//     der Code neu generiert wird.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace PentaWork.Xrm.PowerShell.XrmProxies.Templates.CSharp
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Microsoft.Xrm.Sdk.Metadata;
    using PentaWork.Xrm.PowerShell.XrmProxies.Model;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Users\GerritGazic\Git\PentaWork.Xrm.PowerShell\src\PentaWork.Xrm.PowerShell\XrmProxies\Templates\CSharp\BaseProxy.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class BaseProxy : BaseProxyBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("using Microsoft.Xrm.Sdk;\r\nusing System;\r\nusing System.Collections;\r\nusing System." +
                    "Collections.Generic;\r\nusing System.Linq;\r\n\r\nnamespace ");
            
            #line 14 "C:\Users\GerritGazic\Git\PentaWork.Xrm.PowerShell\src\PentaWork.Xrm.PowerShell\XrmProxies\Templates\CSharp\BaseProxy.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(ProxyNamespace));
            
            #line default
            #line hidden
            this.Write("\r\n{\r\n    public abstract class BaseProxy : Entity\r\n    {\r\n        private readonl" +
                    "y AttributeEqualityComparer _equalityComparer = new AttributeEqualityComparer();" +
                    "\r\n\r\n        protected BaseProxy(Entity original)\r\n        {\r\n            Id = or" +
                    "iginal.Id;\r\n            LogicalName = original.LogicalName;\r\n            Related" +
                    "Entities.Clear();\r\n            FormattedValues.Clear();\r\n            Attributes." +
                    "Clear();\r\n            RelatedEntities.AddRange(original.RelatedEntities);\r\n     " +
                    "       FormattedValues.AddRange(original.FormattedValues);\r\n            Extensio" +
                    "nData = original.ExtensionData;\r\n            Attributes.AddRange(original.Attrib" +
                    "utes);\r\n            EntityState = original.EntityState;\r\n        }\r\n    \r\n      " +
                    "  public Entity GetChangedEntity(bool resetChanges = false)\r\n        {\r\n        " +
                    "    var entity = new Entity(LogicalName)\r\n            {\r\n                Id = Id" +
                    "\r\n            };\r\n            foreach (string attributeName in ChangedValues.Key" +
                    "s)\r\n                entity.Attributes[attributeName] = Attributes[attributeName]" +
                    ";\r\n            if (resetChanges) ChangedValues.Clear();\r\n            return enti" +
                    "ty;\r\n        }\r\n\r\n        protected void SetAttributeValueTracked<T>(string attr" +
                    "ibuteName, T value)\r\n        {\r\n            var currentValue = GetAttributeValue" +
                    "<T>(attributeName);\r\n            if (!_equalityComparer.Equals(currentValue, val" +
                    "ue))\r\n            {\r\n                if (ChangedValues.ContainsKey(attributeName" +
                    ") && _equalityComparer.Equals(ChangedValues[attributeName], value))\r\n           " +
                    "     {\r\n                    ChangedValues.Remove(attributeName);\r\n              " +
                    "  }\r\n                else if(!ChangedValues.ContainsKey(attributeName))\r\n       " +
                    "         {\r\n                    ChangedValues.Add(attributeName, GetAttributeVal" +
                    "ue<T>(attributeName));\r\n                }\r\n                SetAttributeValue(att" +
                    "ributeName, value); \r\n            }\r\n        }\r\n\r\n        public string PrimaryI" +
                    "dAttribute\r\n        {\r\n            get\r\n            {\r\n                var field" +
                    " = GetType().GetFields().SingleOrDefault(f => f.Name == \"PrimaryIdAttribute\");\r\n" +
                    "                return (string)field?.GetValue(this);\r\n            }\r\n        }\r" +
                    "\n\r\n        public string PrimaryNameAttribute\r\n        {\r\n            get\r\n     " +
                    "       {\r\n                var field = GetType().GetFields().SingleOrDefault(f =>" +
                    " f.Name == \"PrimaryNameAttribute\");\r\n                return (string)field?.GetVa" +
                    "lue(this);\r\n            }\r\n        }\r\n\r\n        public bool IsDirty => ChangedVa" +
                    "lues.Count > 0;\r\n\r\n        public Dictionary<string, object> ChangedValues { get" +
                    "; } = new Dictionary<string, object>();\r\n    }\r\n\r\n    public class AttributeEqua" +
                    "lityComparer : IEqualityComparer\r\n    {\r\n        public new bool Equals(object x" +
                    ", object y)\r\n        {\r\n            if ((x == null || (x.GetType() == typeof(str" +
                    "ing) && string.IsNullOrEmpty(x as string))) && (y == null || (y.GetType() == typ" +
                    "eof(string) && string.IsNullOrEmpty(y as string))))\r\n            {\r\n            " +
                    "    return true;\r\n            }\r\n            else\r\n            {\r\n              " +
                    "  if (x == null && y == null) { return true; }\r\n                else if (x == nu" +
                    "ll && y != null) { return false; }\r\n                else if (x != null && y == n" +
                    "ull) { return false; }\r\n                else if (x.GetType() == y.GetType())\r\n  " +
                    "              {\r\n                    if (x.GetType() == typeof(OptionSetValue)) " +
                    "{ return ((OptionSetValue)x).Value == ((OptionSetValue)y).Value; }\r\n            " +
                    "        else if (x.GetType() == typeof(BooleanManagedProperty)) { return ((Boole" +
                    "anManagedProperty)x).Value == ((BooleanManagedProperty)y).Value; }\r\n            " +
                    "        else if (x.GetType() == typeof(EntityReference))\r\n                    {\r" +
                    "\n                        if (((EntityReference)x).LogicalName == ((EntityReferen" +
                    "ce)y).LogicalName) { return ((EntityReference)x).Id == ((EntityReference)y).Id; " +
                    "}\r\n                        else { return false; }\r\n                    }\r\n      " +
                    "              else if (x.GetType() == typeof(Money)) { return ((Money)x).Value =" +
                    "= ((Money)y).Value; }\r\n                    else if (x.GetType() == typeof(DateTi" +
                    "me) || x.GetType() == typeof(DateTime?))\r\n                    {\r\n               " +
                    "         //Compare only down to the second since CRM only saves down to the seco" +
                    "nd\r\n                        return Math.Abs(((DateTime)x - (DateTime)y).TotalSec" +
                    "onds) < 1;\r\n                    }\r\n                    else { return x.Equals(y)" +
                    "; }\r\n                }\r\n                else { return false; }\r\n            }\r\n " +
                    "       }\r\n\r\n        public int GetHashCode(object obj)\r\n        {\r\n            r" +
                    "eturn obj.GetHashCode();\r\n        }\r\n    }\r\n}\r\n\r\n\r\n");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 128 "C:\Users\GerritGazic\Git\PentaWork.Xrm.PowerShell\src\PentaWork.Xrm.PowerShell\XrmProxies\Templates\CSharp\BaseProxy.tt"

public string ProxyNamespace { get; set; }

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public class BaseProxyBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}