using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Model
{
    public class StorageFrame
    {
        public StorageFrame(MethodDef methodDef, StorageFrame? parentFrame = null)
        {
            MethodDef = methodDef;

            if (parentFrame != null)
            {
                ParentFrame = parentFrame;
                CallStack = new Stack<string>(parentFrame.CallStack);
            }
        }

        public StorageFrame? ParentFrame { get; }

        public MethodDef MethodDef { get; }
        public Stack<string> CallStack { get; } = new();
        public List<XrmApiCall> ApiCalls { get; } = new();

        public object[] LocalVars { get; } = new object[255];
        public Stack<object> Stack { get; private set; } = new();
    }
}
