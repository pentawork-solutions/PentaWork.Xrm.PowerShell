﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion: 17.0.0.0
//  
//     Änderungen an dieser Datei können fehlerhaftes Verhalten verursachen und gehen verloren, wenn
//     der Code neu generiert wird.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace PentaWork.Xrm.PowerShell.XrmProxies.Templates.Javascript
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using PentaWork.Xrm.PowerShell.XrmProxies.Model;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Users\GerritGazic\Github\PentaWork.Xrm.PowerShell\src\PentaWork.Xrm.PowerShell\XrmProxies\Templates\Javascript\ProxyTypes.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    internal partial class ProxyTypes : ProxyTypesBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("export interface FormName {\r\n    name: string;\r\n}\r\n\r\nexport interface TabName {\r\n" +
                    "    name: string;\r\n}\r\n\r\nexport interface AttributeName {\r\n    name: string;\r\n}\r\n" +
                    "\r\nexport class AttributeProxy<T, S extends Xrm.Attributes.Attribute, U extends X" +
                    "rm.Controls.Control> {\r\n    constructor(private _formContext: Xrm.FormContext, p" +
                    "rivate _attributeName: AttributeName) { }\r\n\r\n    get LogicalName() {\r\n        re" +
                    "turn this._attributeName.name;\r\n    }\r\n\r\n    get Attribute(): S {\r\n        let a" +
                    "ttribute: S | undefined = this._formContext.getAttribute<S>(this._attributeName." +
                    "name);\r\n        if(attribute === undefined) {\r\n            console.log(`GetAttri" +
                    "bute: No attribute with name \'${this._attributeName.name}\' available on current " +
                    "form!`);\r\n        }\r\n        return attribute;\r\n    }\r\n\r\n    get Control(): U {\r" +
                    "\n        let control: U | undefined = this._formContext.getControl<U>(this._attr" +
                    "ibuteName.name);\r\n        if(control === undefined) {\r\n            console.log(`" +
                    "GetControl: No attribute control with name \'${this._attributeName.name}\' availab" +
                    "le on current form!`);\r\n        }\r\n        return control;\r\n    }\r\n\r\n    get Val" +
                    "ue(): T | undefined {\r\n        let value: T | undefined;\r\n        if(this._formC" +
                    "ontext.getAttribute(this._attributeName.name) !== undefined) {\r\n            valu" +
                    "e = <T | undefined>this._formContext.getAttribute(this._attributeName.name).getV" +
                    "alue();\r\n        }\r\n        else {\r\n            console.log(`Get: No attribute w" +
                    "ith name \'${this._attributeName.name}\' available on current form!`);\r\n        }\r" +
                    "\n        return value;\r\n    }\r\n\r\n    set Value(value: T | undefined) {\r\n        " +
                    "if(this._formContext.getAttribute(this._attributeName.name) !== undefined) {\r\n  " +
                    "          this._formContext.getAttribute(this._attributeName.name).setValue(valu" +
                    "e);\r\n        }\r\n        else {\r\n            console.log(`Set: No attribute with " +
                    "name \'${this._attributeName.name}\' available on current form!`);\r\n        }\r\n   " +
                    " }\r\n}\r\n\r\nexport abstract class FormProxy { \r\n    private _xrm!: Xrm.XrmStatic;\r\n" +
                    "    private _formContext!: Xrm.FormContext;\r\n\r\n    constructor(xrm: Xrm.XrmStati" +
                    "c, formContext: Xrm.FormContext) {\r\n        this._xrm = xrm;\r\n        this._form" +
                    "Context = formContext;\r\n    }\r\n\r\n    /**\r\n     * Asynchronously saves the record" +
                    ".\r\n     * @returns Returns an asynchronous promise.\r\n     */\r\n    public save():" +
                    " Promise<void> {\r\n        return new Promise((resolve, reject) => {\r\n           " +
                    " this.Xrm.Page.data.save()\r\n            .then(resolve, reject);\r\n        });\r\n  " +
                    "  }\r\n\r\n    /**\r\n     * Asynchronously refreshes data on the form, without reload" +
                    "ing the page.\r\n     * @param save true to save the record, after the refresh.\r\n " +
                    "    * @returns Returns an asynchronous promise.\r\n     */\r\n    public refresh(sav" +
                    "e: boolean): Promise<void> {        \r\n        return new Promise((resolve, rejec" +
                    "t) => {\r\n            this.Xrm.Page.data.refresh(save)\r\n            .then(resolve" +
                    ", reject);\r\n        });\r\n    }\r\n\r\n    /**\r\n     * @returns The current entity id" +
                    ".\r\n     */  \r\n    public getEntityId(removeBraces: boolean = false) {\r\n        i" +
                    "f(removeBraces) return this._formContext.data.entity.getId().replace(/[{}]/g, \"\"" +
                    ");\r\n        else return this._formContext.data.entity.getId();\r\n    }\r\n\r\n    /**" +
                    "\r\n     * Gets form type.\r\n     * @returns The form type.\r\n     * @remarks **Valu" +
                    "es returned are**:\r\n     * * 0  Undefined\r\n     * * 1  Create\r\n     * * 2  Updat" +
                    "e\r\n     * * 3  Read Only\r\n     * * 4  Disabled\r\n     * * 6  Bulk Edit\r\n     * * " +
                    "Deprecated values are 5 (Quick Create), and 11 (Read Optimized)\r\n     */\r\n    ge" +
                    "t FormType(): XrmEnum.FormType {\r\n        return this._formContext.ui.getFormTyp" +
                    "e();\r\n    }\r\n\r\n    /**\r\n     * Gets the label for the form.\r\n     * @returns The" +
                    " FormName.\r\n     */\r\n    get FormName(): FormName {\r\n        return { name: this" +
                    "._formContext.ui.formSelector.getCurrentItem().getLabel() };\r\n    }  \r\n\r\n    /**" +
                    "\r\n     * @returns The current Xrm Object on this script\r\n     */  \r\n    get Xrm(" +
                    ") {\r\n        return this._xrm;\r\n    }\r\n\r\n    /**\r\n     * @return The current For" +
                    "m Context\r\n     */\r\n    get FormContext() {\r\n        return this._formContext;\r\n" +
                    "    }\r\n}");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    internal class ProxyTypesBase
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
        public System.Text.StringBuilder GenerationEnvironment
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
