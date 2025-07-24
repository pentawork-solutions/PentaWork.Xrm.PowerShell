using dnlib.DotNet;
using dnlib.DotNet.Emit;
using PentaWork.Xrm.PluginGraph.Hooks;
using PentaWork.Xrm.PluginGraph.Model;
using PentaWork.Xrm.PluginGraph.Model.VMObjects;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph
{
    /// <summary>
    /// The PluginGraphVM is a simple interpreter for ILCode.
    /// The goal of this VM is to check for used Power Platform API Calls and 
    /// the used objects, to be able to analyze which plugins are potential calling other plugins.
    /// Together with the registered plugin step information of an Power Platform environment, 
    /// this information can then be used to create a diagram of plugins, their registration and triggers.
    /// </summary>
    public class PluginGraphVM
    {
        private readonly PluginGraphVMData _vmData;
        private readonly List<IHook> _callHooks;
        private readonly List<IHook> _creationHooks;

        public PluginGraphVM(PluginModuleList moduleList)
        {
            _vmData = new PluginGraphVMData(moduleList);

            _callHooks = GetType().Assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.Name == "ICallHook"))
                .Select(t => (IHook)Activator.CreateInstance(t)!)
                .ToList();

            _creationHooks = GetType().Assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.Name == "ICreationHook"))
                .Select(t => (IHook)Activator.CreateInstance(t)!)
                .ToList();
        }

        /// <summary>
        /// Executes the given MethodDef. Already parsed into instructions by dnlib.
        /// </summary>
        /// <param name="methodDef"><c>MethodDef</c> parsed by dnlib</param>
        /// <param name="parameters">A list of parameters for the called method. 
        /// If used without any parameters, ldarg will push a dummy value on the stack.</param>
        /// <returns></returns>
        public (List<XrmApiCall>, object?) Execute(MethodDef methodDef, List<object>? parameters = null)
        {
            var index = 0;
            var instructions = methodDef.Body.Instructions;
            while (index < instructions.Count)
            {
                var instr = instructions[index++];
                switch (instr.OpCode.Code)
                {
                    case Code.Ldarg_0:
                        _vmData.Stack.Push(parameters?.Count > 0 ? parameters[0] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Load argument 0 onto stack");
                        break;
                    case Code.Ldarg_1:
                        _vmData.Stack.Push(parameters?.Count > 1 ? parameters[1] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Load argument 1 onto stack");
                        break;
                    case Code.Ldarg_2:
                        _vmData.Stack.Push(parameters?.Count > 2 ? parameters[2] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Load argument 2 onto stack");
                        break;
                    case Code.Ldarg_3:
                        _vmData.Stack.Push(parameters?.Count > 3 ? parameters[3] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Load argument 3 onto stack");
                        break;
                    case Code.Stloc_0:
                        _vmData.LocalVars[0] = _vmData.Stack.Pop();
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 0");
                        break;
                    case Code.Stloc_1:
                        _vmData.LocalVars[1] = _vmData.Stack.Pop();
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 1");
                        break;
                    case Code.Stloc_2:
                        _vmData.LocalVars[2] = _vmData.Stack.Pop();
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 2");
                        break;
                    case Code.Stloc_3:
                        _vmData.LocalVars[3] = _vmData.Stack.Pop();
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 3");
                        break;
                    case Code.Stloc:
                    case Code.Stloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _vmData.LocalVars[operand.Index] = _vmData.Stack.Pop();
                            Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Pops a value from the stack and stores it in local variable index");
                            break;
                        }
                    case Code.Ldloc_0:
                        _vmData.Stack.Push(_vmData.LocalVars[0]);
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Loads the local variable at index 0 onto the evaluation stack");
                        break;
                    case Code.Ldloc_1:
                        _vmData.Stack.Push(_vmData.LocalVars[1]);
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Loads the local variable at index 1 onto the evaluation stack");
                        break;
                    case Code.Ldloc_2:
                        _vmData.Stack.Push(_vmData.LocalVars[2]);
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Loads the local variable at index 2 onto the evaluation stack");
                        break;
                    case Code.Ldloc_3:
                        _vmData.Stack.Push(_vmData.LocalVars[3]);
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Loads the local variable at index 3 onto the evaluation stack");
                        break;
                    case Code.Ldloc:
                    case Code.Ldloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _vmData.Stack.Push(_vmData.LocalVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Loads the local variable at index index onto stack");
                            break;
                        }
                    case Code.Ldloca:
                    case Code.Ldloca_S:
                        {
                            // Normally the adress of a local variable would get pushed onto the stack.
                            // We are not interested in actual values. Therefore we just init the local variable directly.
                            var operand = (Local)instr.Operand;
                            _vmData.LocalVars[operand.Index] = $"Dummy Value for '{instr.OpCode.ToString()}'";
                            _vmData.Stack.Push(_vmData.LocalVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Loads the address of the local variable at index onto the evaluation stack");
                            break;
                        }
                    case Code.Ldstr:
                        {
                            var operand = (string)instr.Operand;
                            _vmData.Stack.Push(operand);
                            Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} {operand} // Pushes a string object for the metadata string token mdToken");
                            break;
                        }
                    case Code.Dup:
                        var obj = _vmData.Stack.Peek();
                        if (obj is IVMObj) _vmData.Stack.Push(obj);
                        else _vmData.Stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Duplicates the value on the top of the stack.");
                        break;
                    case Code.Newobj:
                        Debug.WriteLine($"- [Start {instr.ToString()}]");
                        HandleCall(instr, _creationHooks, true);
                        Debug.WriteLine($"- [End {instr.ToString()}]");
                        break;
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        Debug.WriteLine($"- [Start {instr.ToString()}]");
                        HandleCall(instr, _callHooks);
                        Debug.WriteLine($"- [End {instr.ToString()}]");
                        break;
                    #region Dummy Stack Manipulations
                    case Code.Ldc_I4:
                    case Code.Ldc_I4_0:
                    case Code.Ldc_I4_1:
                    case Code.Ldc_I4_2:
                    case Code.Ldc_I4_3:
                    case Code.Ldc_I4_4:
                    case Code.Ldc_I4_5:
                    case Code.Ldc_I4_6:
                    case Code.Ldc_I4_7:
                    case Code.Ldc_I4_8:
                    case Code.Ldc_I4_M1:
                    case Code.Ldc_I4_S:
                    case Code.Ldc_I8:
                    case Code.Ldc_R4:
                    case Code.Ldc_R8:
                    case Code.Ldnull:
                    case Code.Ldtoken:
                    case Code.Ldftn:
                    case Code.Ldsfld:
                        _vmData.Stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] {instr.ToString()} // Push a value on the stack");
                        break;
                    case Code.Ldfld:
                        _vmData.Stack.Pop();
                        _vmData.Stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[∙ {_vmData.Stack.Count}] {instr.ToString()} // Pushes the value of a field in a specified object onto the stack (pops the object reference during operation)");
                        break;
                    case Code.Ceq:
                    case Code.Cgt:
                    case Code.Cgt_Un:
                    case Code.And:
                    case Code.Or:
                    case Code.Xor:
                        _vmData.Stack.Pop();
                        _vmData.Stack.Pop();
                        _vmData.Stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Compares two values and pushes result");
                        break;
                    case Code.Stfld:
                    case Code.Beq:
                    case Code.Beq_S:
                        _vmData.Stack.Pop();
                        _vmData.Stack.Pop();
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Acts based on two values");
                        break;
                    case Code.Pop:
                    case Code.Stsfld:
                    case Code.Throw:
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        _vmData.Stack.Pop();
                        Debug.WriteLine($"[↓ {_vmData.Stack.Count}] {instr.ToString()} // Pops a value for its operation");
                        break;
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Br:
                    case Code.Br_S:
                        {
                            var operand = (Instruction)instr.Operand;
                            while (instructions[index].Offset != operand.Offset) index++;
                            Debug.WriteLine($"[∙ {_vmData.Stack.Count}] {instr.ToString()} // Jumps to another code part");
                            break;
                        }
                    #endregion
                    case Code.Nop:
                    case Code.Ret:
                    case Code.Castclass:
                        Debug.WriteLine($"[∙ {_vmData.Stack.Count}] {instr.ToString()}");
                        break;
                    default:
                        Debug.WriteLine($"NOT IMPLEMENTED {instr.OpCode.ToString()}");
                        break;
                }
            }

            return (_vmData.ApiCalls, _vmData.Stack.Count > 0 ? _vmData.Stack.Pop() : null);
        }

        private void HandleCall(Instruction instr, List<IHook> hooks, bool isNewObj = false)
        {
            var method = (IMethod)instr.Operand;
            var methodDef = _vmData.ModuleList.TryFindMethod(method.DeclaringType.FullName, method.FullName);

            // Get all parameters from the current stack
            var parameters = new List<object>();
            for (var i = 0; i < method.MethodSig.GetParamCount(); i++) parameters.Add(_vmData.Stack.Pop());

            // If not a static call, pop 'this' from the stack
            if (!isNewObj && method.MethodSig.HasThis) parameters.Add(_vmData.Stack.Pop());

            // Reverse List to have the parameters in order: this, param1, param2 ....
            parameters.Reverse();
            if (parameters.Any()) Debug.WriteLine($"[↓ {_vmData.Stack.Count}] Popped method arguments");

            var hookExecuted = false;
            foreach (var hook in hooks)
            {
                if (hook.HookApplicable(method, methodDef, parameters))
                {
                    hook.ExecuteHook(_vmData, method, methodDef, parameters);
                    hookExecuted = true;
                    Debug.WriteLine($"[∙ {_vmData.Stack.Count}] Hook executed!");
                    break;
                }
            }

            if (!hookExecuted && isNewObj)
            {
                // get the constructor parameters from the stack
                _vmData.Stack.Push($"New Object ({method.FullName})");
            }
            else if (!hookExecuted && !isNewObj)
            {
                // The called method is part of the loaded dlls
                // Interpret it to get more call information
                if (methodDef != null && methodDef.Body != null)
                {
                    var vm = new PluginGraphVM(_vmData.ModuleList);
                    var (apicalls, returnValue) = vm.Execute(methodDef, parameters);

                    _vmData.ApiCalls.AddRange(apicalls);
                    if (returnValue != null)
                    {
                        _vmData.Stack.Push(returnValue);
                        Debug.WriteLine($"[↑ {_vmData.Stack.Count}] Return value from {methodDef.FullName}");
                    }
                }
                else if (method.MethodSig.RetType.FullName != "System.Void")
                {
                    _vmData.Stack.Push($"Dummy return value for '{method.FullName}'");
                    Debug.WriteLine($"[↑ {_vmData.Stack.Count}] Return value from {method.FullName}");
                }
            }
            if (isNewObj) Debug.WriteLine($"[↑ {_vmData.Stack.Count}] New object {method.FullName}");
        }
    }
}
