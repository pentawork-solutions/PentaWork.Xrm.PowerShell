using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Model
{
    public class PluginGraphVMData
    {
        public PluginGraphVMData(PluginModuleList moduleList)
        {
            ModuleList = moduleList;
        }

        public PluginModuleList ModuleList { get; }
        public Stack<object> Stack { get; } = new();
        public object[] LocalVars { get; } = new object[255];
        public List<XrmApiCall> ApiCalls { get; } = new();
    }
}
