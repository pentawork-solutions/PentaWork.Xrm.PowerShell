using Microsoft.Xrm.Sdk;
using System.Linq;

namespace PentaWork.Xrm.PowerShell.XrmProxies
{
    public static class LabelExtensions
    {
        /// <summary>
        /// Returns the localized label of the given XRM Label.
        /// </summary>
        /// <param name="label">The label to get the localized value from.</param>
        /// <param name="fallback">The fallback label, if no label is existing.</param>
        /// <returns>The localized label.</returns>
        public static string GetLabel(this Label label, string fallback = "")
        {
            if (label.LocalizedLabels.Count == 0) return fallback;

            var localizedLabel = label.LocalizedLabels.SingleOrDefault(l => l.LanguageCode == 1033);
            localizedLabel = localizedLabel ?? label.LocalizedLabels.FirstOrDefault();
            return localizedLabel.Label?.Replace("\"", "'").Trim();
        }
    }
}
