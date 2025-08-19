namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class ExecutionContextObj : IVMObj
    {
        public EntityObj GetItem(string name)
        {
            return new EntityObj
            {
                IsTarget = name == "Target"
            };
        }
    }
}
