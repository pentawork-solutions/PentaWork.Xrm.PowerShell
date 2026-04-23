using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    public class MessageGraph
    {
        public MessageGraph(string message)
        {
            Message = message;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            if (!Stages.ContainsKey(pluginStepInfo.Stage))
            {
                Stages.Add(pluginStepInfo.Stage, new List<PluginStepInfo>());
            }
            Stages[pluginStepInfo.Stage].Add(pluginStepInfo);
        }

        public string Message { get; }
        public Dictionary<Stage, List<PluginStepInfo>> Stages { get; } = new();
    }
}
