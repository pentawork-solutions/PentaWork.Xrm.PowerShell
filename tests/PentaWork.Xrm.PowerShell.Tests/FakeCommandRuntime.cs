using System.Management.Automation;
using System.Management.Automation.Host;

namespace PentaWork.Xrm.PowerShell.Tests
{
    /// <summary>
    /// Minimal no-op ICommandRuntime so a PSCmdlet can be invoked directly in a unit test -
    /// without this, any WriteProgress/WriteObject/etc. call throws NotImplementedException
    /// because the cmdlet isn't running inside a real PowerShell pipeline.
    /// </summary>
    internal class FakeCommandRuntime : ICommandRuntime
    {
        public PSHost Host => null!;
        public PSTransactionContext CurrentPSTransaction => null!;

        public void WriteDebug(string text) { }
        public void WriteError(ErrorRecord errorRecord) { }
        public void WriteObject(object sendToPipeline) { }
        public void WriteObject(object sendToPipeline, bool enumerateCollection) { }
        public void WriteProgress(ProgressRecord progressRecord) { }
        public void WriteProgress(long sourceId, ProgressRecord progressRecord) { }
        public void WriteVerbose(string text) { }
        public void WriteWarning(string text) { }
        public void WriteCommandDetail(string text) { }
        public bool ShouldProcess(string target) => true;
        public bool ShouldProcess(string target, string action) => true;
        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) => true;
        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            shouldProcessReason = ShouldProcessReason.None;
            return true;
        }
        public bool ShouldContinue(string query, string caption) => true;
        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) => true;
        public bool TransactionAvailable() => false;
        public void ThrowTerminatingError(ErrorRecord errorRecord) => throw new System.Exception(errorRecord.ToString());
    }
}
