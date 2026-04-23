namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class XrmApiCall : IVMObj
    {
        public string? Message { get; set; }
        public EntityObj? EntityInfo { get; set; }

        public bool IsTarget { get; set; } = false;
        public bool IsExecuted { get; set; } = true;
    }
}
