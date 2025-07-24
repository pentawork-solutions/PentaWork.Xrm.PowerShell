using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph
{
    public class PluginGraphAnalyzer
    {
        public PluginGraphAnalyzer(string pluginPath)
        {
            PluginPath = pluginPath;
        }

        public Dictionary<string, List<XrmApiCall>> Analyze(List<string>? pluginTypeFullNames = null)
        {
            var apiCalls = new Dictionary<string, List<XrmApiCall>>();

            var assemblyList = Directory.GetFiles(PluginPath, "*.dll");
            foreach (var assemblyFile in assemblyList)
            {
                var module = ModuleDefMD.Load(assemblyFile);
                ModuleList.Add(module);
            }
            Debug.WriteLine($"Found {ModuleList.Count} assemblies...");

            var pluginTypes = ModuleList.SelectMany(m => m.GetTypes().Where(t => t.Interfaces.Any(i => i.Interface.Name == "IPlugin")));
            Debug.WriteLine($"Found {pluginTypes.Count()} plugins...");

            foreach (var pluginType in pluginTypes.Where(p => pluginTypeFullNames == null || pluginTypeFullNames.Contains(p.FullName)))
            {
                var executeMethod = pluginType.Methods.SingleOrDefault(m => m.Name == "Execute");
                if (executeMethod == null) continue;

                var instructions = executeMethod.Body.Instructions;

                var vm = new PluginGraphVM(ModuleList);
                apiCalls.Add(pluginType.FullName, vm.Execute(instructions).Item1);
            }

            return apiCalls;
        }

        public string PluginPath { get; }
        public PluginModuleList ModuleList { get; } = new();
    }
}
