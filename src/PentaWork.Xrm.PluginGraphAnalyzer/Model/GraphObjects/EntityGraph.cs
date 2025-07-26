using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    internal class EntityGraph
    {
        public EntityGraph(string entityName)
        {
            EntityName = entityName;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            var message = Messages.SingleOrDefault(e => e.Message == pluginStepInfo.SdkMessage.Name);
            if (message == null)
            {
                message = new MessageGraph(pluginStepInfo.SdkMessage.Name);
                Messages.Add(message);
            }
            message.Add(pluginStepInfo);
        }

        public string EntityName { get; }
        public List<MessageGraph> Messages { get; } = new();
    }
}
