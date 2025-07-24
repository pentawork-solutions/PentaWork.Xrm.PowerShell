namespace PentaWork.Xrm.PluginGraph.Model
{
    public class XrmApiCall
    {
        public string Message { get; set; }
        public EntityObj EntityInfo { get; set; }

        public bool IsTarget { get; set; } = false;
        public bool IsExecuted { get; set; } = true;
    }
}
