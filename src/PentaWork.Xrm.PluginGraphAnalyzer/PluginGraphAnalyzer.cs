using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PluginGraph.Extensions;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.GraphObjects;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph
{
    public class PluginGraphAnalyzer
    {
        private readonly PluginModuleList _moduleList = new();

        public void AnalyzeSystem(CrmServiceClient connection, Guid? solutionId = null)
        {
            IEnumerable<ComponentInfo>? solutionComponents = null;
            if (solutionId != null)
            {
                // We are fetching the solution components first, instead of filtering the plugin types based on the solution id.
                // This way this module also works for unmanaged solutions -> Unmanaged Plugin Types are part of the default solution.
                solutionComponents = connection
                    .QueryEntity("solutioncomponent", true, new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId))
                    .Select(e => new ComponentInfo(e));
            }
            var pluginsStepInfos = connection.GetPluginSteps(solutionComponents);

            var entityGraphList = new EntityGraphList();
            pluginsStepInfos.ToList().ForEach(entityGraphList.Add);
        }

        /*    public Dictionary<string, List<XrmApiCall>> AnalyzePluginStepInfos(IEnumerable<PluginStepInfo> pluginStepInfos)
            {
                var entityGraphList = new EntityGraphList();
                _pluginStepInfos.ToList().ForEach(entityGraphList.Add);

                return AnalyzeApiCalls(pluginTypeFullNames);
            } */




        private Dictionary<string, List<XrmApiCall>> AnalyzeApiCalls(List<string>? pluginTypeFullNames = null)
        {
            var apiCalls = new Dictionary<string, List<XrmApiCall>>();

            /* var assemblyList = Directory.GetFiles(PluginPath, "*.dll");
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

                var vm = new PluginGraphVM(ModuleList);
                apiCalls.Add(pluginType.FullName, vm.Execute(executeMethod).Item1);
            }*/

            return apiCalls;
        }

    }
}
