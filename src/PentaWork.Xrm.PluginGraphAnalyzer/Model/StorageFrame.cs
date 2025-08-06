using dnlib.DotNet;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;

namespace PentaWork.Xrm.PluginGraph.Model
{
    public class StorageFrame
    {
        private readonly Stack<Stack<object>> _savedStack = new();

        public StorageFrame(MethodDef methodDef, List<object>? parameters = null, StorageFrame? parentFrame = null)
        {
            MethodDef = methodDef;
            Parameters = parameters;

            if (parentFrame != null)
            {
                ParentFrame = parentFrame;
                CallStack = new Stack<string>(parentFrame.CallStack.Reverse()); // Necessary, because the ctor iterates over the items, so we have to feed them in reverse
            }
        }

        public void SaveStack()
        {
            _savedStack.Push(new Stack<object>(Stack.Reverse()));
        }

        public void RestoreStack()
        {
            if (_savedStack.Count == 0) throw new Exception("No Stack Saved!");
            Stack = _savedStack.Pop();
        }

        public StorageFrame? ParentFrame { get; }
        public List<object>? Parameters { get; }

        public MethodDef MethodDef { get; }
        public Stack<string> CallStack { get; } = new();
        public List<XrmApiCall> ApiCalls { get; } = new();

        public object[] LocalVars { get; } = new object[255];
        public Stack<object> Stack { get; private set; } = new();
    }
}
