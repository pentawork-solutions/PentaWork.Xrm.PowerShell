using dnlib.DotNet;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using PentaWork.Xrm.PluginGraph.Extensions;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.GraphObjects;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;
using System.IO.Compression;

namespace PentaWork.Xrm.PluginGraph
{
    public class PluginGraphAnalyzer
    {
        public EntityGraphList AnalyzeSystem(CrmServiceClient connection, Guid solutionId, string namespaces, bool log = false)
        {
            var solutionComponents = GetSolutionComponents(connection, solutionId);
            var pluginsStepInfos = connection.GetPluginSteps(solutionComponents);

            var moduleLists = LoadModules(connection, pluginsStepInfos);
            var apiCalls = AnalyzeApiCalls(moduleLists, pluginsStepInfos, namespaces, log);

            var entityGraphList = new EntityGraphList(apiCalls);
            pluginsStepInfos.ToList().ForEach(entityGraphList.Add);

            return entityGraphList;
        }

        public Dictionary<string, List<XrmApiCall>> AnalyzeApiCalls(Dictionary<Guid, PluginModuleList> moduleLists, IEnumerable<PluginStepInfo> pluginStepInfos, string namespaces, bool log = false)
        {
            var apiCalls = new Dictionary<string, List<XrmApiCall>>();

            foreach (var pluginStepInfo in pluginStepInfos)
            {
                if (apiCalls.ContainsKey(pluginStepInfo.Plugin.TypeName)) continue;

                var moduleId = pluginStepInfo.Plugin!.PackageInfo != null
                    ? pluginStepInfo.Plugin.PackageInfo.Id
                    : pluginStepInfo.Plugin.AssemblyInfo!.Id;
                var moduleList = moduleLists[moduleId];
                var pluginType = moduleList
                    .Single(m => m.Assembly.Name == pluginStepInfo.Plugin.AssemblyInfo.Name)
                    .GetTypes()
                    .Single(t => t.FullName == pluginStepInfo.Plugin.TypeName);

                var methodDef = pluginType.Methods.SingleOrDefault(m => m.Name == "Execute");
                if (methodDef == null)
                {
                    var baseType = pluginType.BaseType?.ResolveTypeDef();
                    while (baseType != null)
                    {
                        methodDef = baseType.Methods.SingleOrDefault(m => m.Name == "Execute");
                        if (methodDef != null) break;
                        else baseType = baseType.BaseType?.ResolveTypeDef();
                    }
                }

                var vm = new PluginGraphVM(moduleLists[moduleId], namespaces.Split(new char[','], StringSplitOptions.RemoveEmptyEntries).ToList(), log);
                apiCalls.Add(pluginStepInfo.Plugin.TypeName, vm.Execute(methodDef, null, [new GenericObj(pluginType.FullName, pluginType)]).Item1);
            }

            return apiCalls;
        }

        private IEnumerable<ComponentInfo>? GetSolutionComponents(CrmServiceClient connection, Guid? solutionId)
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
            return solutionComponents;
        }

        private Dictionary<Guid, PluginModuleList> LoadModules(CrmServiceClient connection, IEnumerable<PluginStepInfo> pluginsStepInfos)
        {
            var packageIds = pluginsStepInfos
                .Where(p => p.Plugin?.PackageInfo != null)
                .Select(p => p.Plugin!.PackageInfo!.Id)
                .Distinct()
                .ToList();
            var assemblyIds = pluginsStepInfos
                .Where(p => p.Plugin?.PackageInfo == null)
                .Select(p => p.Plugin!.PackageInfo!.Id)
                .Distinct()
                .ToList();

            var packages = packageIds.Select(p => (p, connection.DownloadFile(new EntityReference("pluginpackage", p), "package")));
            var assemblies = assemblyIds.Select(a => (a, Convert.FromBase64String((string)connection.Retrieve("pluginassembly", a, new ColumnSet("content"))["content"])));
            var tmpPath = Path.Combine(Path.GetTempPath(), "pluginGraphAnalyzer");

            var pluginModuleLists = new Dictionary<Guid, PluginModuleList>();
            foreach (var package in packages)
            {
                Directory.CreateDirectory(tmpPath);
                var zipPath = Path.Combine(tmpPath, "tmp.zip");
                File.WriteAllBytes(zipPath, package.Item2);
                ZipFile.ExtractToDirectory(zipPath, tmpPath);

                var moduleList = new PluginModuleList();
                var ctx = ModuleDef.CreateModuleContext();
                var asmResolver = (AssemblyResolver)ctx.AssemblyResolver;
                asmResolver.EnableTypeDefCache = true;

                var assemblyList = Directory.GetFiles(tmpPath, "*.dll", SearchOption.AllDirectories);
                foreach (var assemblyFile in assemblyList)
                {
                    var module = ModuleDefMD.Load(assemblyFile, ctx);
                    module.Context = ctx;
                    asmResolver.AddToCache(module);

                    moduleList.Add(module);
                }

                Directory.Delete(tmpPath, true);
                pluginModuleLists.Add(package.p, moduleList);
            }

            Directory.CreateDirectory(tmpPath);
            foreach (var assembly in assemblies)
            {
                var assemblyPath = Path.Combine(tmpPath, "tmp.dll");
                File.WriteAllBytes(assemblyPath, assembly.Item2);
                var moduleList = new PluginModuleList { ModuleDefMD.Load(assemblyPath) };

                File.Delete(assemblyPath);
                pluginModuleLists.Add(assembly.a, moduleList);
            }
            Directory.Delete(tmpPath, true);

            return pluginModuleLists;
        }
    }
}
