namespace PentaWork.Xrm.PluginGraph.Model.VMObjects
{
    public class ServiceContextObj : IVMObj
    {
        public void AddCall(XrmApiCall apiCall)
        {
            PendingCalls.Add(apiCall);
        }

        public void ClearQueue()
        {
            PendingCalls.Clear();
        }

        public void MarkCallsExecuted()
        {
            PendingCalls.ForEach(a => a.IsExecuted = true);
        }

        public List<XrmApiCall> PendingCalls { get; } = new();
    }
}
