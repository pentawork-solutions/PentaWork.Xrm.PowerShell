using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace PentaWork.Xrm.PluginGraph.Templates
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

            var context = new TemplateContext
            {
                MemberRenamer = member => member.Name,
                // Scriban's default LoopLimit (1000) is a script-injection safety guard, and it
                // counts cumulatively per loop *statement* across all re-entries - e.g. the
                // "TriggeredBy" loop is re-entered once per plugin step in the whole system, so
                // the combined total across all steps can exceed 1000 even if no single step has
                // anywhere near that many triggers. This is our own trusted, fixed template (not
                // user-supplied script) over inherently finite data, so disable the guard (0).
                LoopLimit = 0
            };
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
