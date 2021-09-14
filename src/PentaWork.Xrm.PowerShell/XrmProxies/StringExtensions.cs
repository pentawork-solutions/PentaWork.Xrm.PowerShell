using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;

namespace PentaWork.Xrm.PowerShell.XrmProxies
{
    public static class StringExtensions
    {
        public static string AsValidVariableName(this string name)
        {
            var validName = name;
            validName = string.Join("", validName                               // NameToCamelCase
                .Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries)   //
                .Select(s => s.FirstToUpper()).ToArray());                      //      
            validName = Regex.Replace(validName, @"\W+", "");                   // Replace non-word characters
            validName = validName.Trim('_');
            if (Regex.IsMatch(validName, @"^\d+")) validName = $"_{validName}"; // Prefix "_" if string starts with number(s)
            validName = string.IsNullOrEmpty(validName) ? "Empty" : validName;
            validName = validName.FirstToUpper();

            var codeDomprovider = CodeDomProvider.CreateProvider("C#");
            return codeDomprovider.IsValidIdentifier(validName) ? validName : $"@{validName}";
        }

        private static string FirstToUpper(this string str)
        {
            if (str == null)
                return null;
            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);
            return str.ToUpper();
        }
    }
}
