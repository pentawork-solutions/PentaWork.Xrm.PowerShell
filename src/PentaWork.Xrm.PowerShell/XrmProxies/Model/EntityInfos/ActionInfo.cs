namespace PentaWork.Xrm.PowerShell.XrmProxies.Model
{
    public class ActionInfo
    {
        public ActionInfo(string uniqueDisplayName, string sdkMessageName, string primaryEntity)
        {
            UniqueDisplayName = uniqueDisplayName;
            SdkMessageName = sdkMessageName;
            PrimaryEntity = primaryEntity;
        }

        public string PrimaryEntity { get; }
        public string SdkMessageName { get; }
        public string UniqueDisplayName { get; }
    }
}
