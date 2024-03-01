using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PentaWork.Xrm.PowerShell.XrmProxies
{
    public static class StringExtensions
    {
        private static Dictionary<string, string> _diacriticsReplacements = new Dictionary<string, string>()
        {
            { "ä", "ae" },
            { "ü", "ue" },
            { "ö", "oe" },
            { "ß", "ss" },
            { "Ä", "Ae" },
            { "Ü", "Ue" },
            { "Ö", "Oe" }
        };

        public static string AsValidVariableName(this string name)
        {
            var validName = name;
            validName = string.Join("", validName                               // NameToCamelCase
                .Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries)   //
                .Select(s => s.FirstToUpper()).ToArray());                      //
            validName = string.Join("", validName                               // 
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)    //
                .Select(s => s.FirstToUpper()).ToArray());                      // 

            validName = Regex.Replace(validName, @"\W+", "");                   // Replace non-word characters
            validName = validName.Trim('_');
            if (Regex.IsMatch(validName, @"^\d+")) validName = $"_{validName}"; // Prefix "_" if string starts with number(s)
            validName = string.IsNullOrEmpty(validName) ? "Empty" : validName;
            validName = validName.RemoveDiacritics();
            validName = validName.FirstToUpper();

            var codeDomprovider = CodeDomProvider.CreateProvider("C#");
            return codeDomprovider.IsValidIdentifier(validName) ? validName : $"@{validName}";
        }

        public static string RemoveDiacritics(this string text)
        {
            // First replace defined diacritics with defined alternatives
            foreach(var diacritic in _diacriticsReplacements)
            {
                if(text.Contains(diacritic.Key)) text = text.Replace(diacritic.Key, diacritic.Value);
            }

            // Replace all other not defined diacritics
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (var i = 0; i < normalizedString.Length; i++)
            {
                var c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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
