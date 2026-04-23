using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;
using PentaWork.Xrm.PluginGraph.Templates;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    public class EntityGraph
    {
        public EntityGraph(string entityName)
        {
            EntityName = entityName;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            var message = Messages.SingleOrDefault(e => e.Message == pluginStepInfo.SdkMessage);
            if (message == null)
            {
                message = new MessageGraph(pluginStepInfo.SdkMessage);
                Messages.Add(message);
            }
            message.Add(pluginStepInfo);
        }

        public string ToMarkdown()
        {
            var mainTemplate = new MainTemplate { EntityGraph = this };
            return mainTemplate.TransformText();
        }

        public string EntityName { get; }
        public List<MessageGraph> Messages { get; } = new();
    }
}
