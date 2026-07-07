using System.Collections.Concurrent;
using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace PentaWork.Xrm.PowerShell.XrmProxies.Templates
{
    internal static class ScribanTemplateRenderer
    {
        private static readonly ConcurrentDictionary<string, Template> _cache = new();
        private static readonly Assembly _assembly = typeof(ScribanTemplateRenderer).Assembly;

        public static string Render(string templateKey, object model)
        {
            var template = _cache.GetOrAdd(templateKey, LoadTemplate);
            if (template.HasErrors)
                throw new InvalidOperationException($"Scriban template '{templateKey}' failed to parse: {string.Join("; ", template.Messages)}");

            var scriptObject = new ScriptObject();
            scriptObject.Import(model, renamer: member => member.Name);

            var context = new TemplateContext { MemberRenamer = member => member.Name };
            context.PushGlobal(scriptObject);
            return template.Render(context);
        }

        private static Template LoadTemplate(string templateKey)
        {
            var resourceName = _assembly.GetManifestResourceNames()
                .SingleOrDefault(n => n.EndsWith($".{templateKey}.sbn", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"Embedded Scriban template '{templateKey}' not found.");

            using var stream = _assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return Template.Parse(reader.ReadToEnd(), templateKey);
        }
    }
}
