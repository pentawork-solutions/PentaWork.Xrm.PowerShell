using Microsoft.Xrm.Sdk;

namespace PentaWork.Xrm.PluginGraph.Model.XrmInfoObjects
{
    public class ComponentInfo
    {
        public ComponentInfo(Entity entity)
        {
            Id = entity.Id;
            ObjectId = (Guid)entity["objectid"];
            ComponentType = ((OptionSetValue)entity["componenttype"]).Value;
        }

        public Guid Id { get; set; }
        public Guid ObjectId { get; set; }
        public int ComponentType { get; set; }
    }
}
