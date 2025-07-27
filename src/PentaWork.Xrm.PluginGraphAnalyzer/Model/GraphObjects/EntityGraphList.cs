using PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects;

namespace PentaWork.Xrm.PluginGraph.Model.GraphObjects
{
    internal class EntityGraphList : List<EntityGraph>
    {
        public void Add(PluginStepInfo pluginStepInfo)
        {
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
