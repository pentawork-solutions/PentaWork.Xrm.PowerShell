using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    public class EntityGraphList : List<EntityGraph>
    {
        private readonly Dictionary<string, List<XrmApiCall>> _apiCalls;

        public EntityGraphList(Dictionary<string, List<XrmApiCall>> apiCalls)
        {
            _apiCalls = apiCalls;
        }

        public void Add(PluginStepInfo pluginStepInfo)
        {
            if (pluginStepInfo.Plugin != null)
                pluginStepInfo.Plugin.ApiCalls = _apiCalls.SingleOrDefault(c => c.Key == pluginStepInfo.Plugin.TypeName).Value;

            var entity = this.SingleOrDefault(e => e.EntityName == pluginStepInfo.PrimaryEntityName);
            if (entity == null)
            {
                entity = new EntityGraph(pluginStepInfo.PrimaryEntityName);
                Add(entity);
            }
            entity.Add(pluginStepInfo);
        }
    }
}
