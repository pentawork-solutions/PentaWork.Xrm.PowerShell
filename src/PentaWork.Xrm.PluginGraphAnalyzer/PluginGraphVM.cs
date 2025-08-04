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
        private readonly List<IHook> _callHooks;
        private readonly PluginModuleList _moduleList;

        private StorageFrame? _storageFrame;

        public PluginGraphVM(PluginModuleList moduleList)
        {
            _moduleList = moduleList;
            _callHooks = GetType().Assembly.GetTypes()
                .Where(type => typeof(IHook).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                .Select(t => (IHook)Activator.CreateInstance(t))
                .ToList();
        }

        /// <summary>
        /// Executes the given MethodDef. Already parsed into instructions by dnlib.
        /// </summary>
        /// <param name="methodDef"><c>MethodDef</c> parsed by dnlib</param>
        /// <param name="parameters">A list of parameters for the called method. 
        /// If used without any parameters, ldarg will push a dummy value on the stack.</param>
        /// <returns></returns>
        public (List<XrmApiCall>, object?) Execute(MethodDef methodDef, List<object>? parameters = null, StorageFrame? parentFrame = null)
        {
            _storageFrame = new StorageFrame(methodDef, parentFrame);
            Execute(methodDef.Body.Instructions, parameters);
            var (apiCalls, returnValue) = (_storageFrame.ApiCalls, _storageFrame.Stack.Count > 0 ? _storageFrame.Stack.Pop() : null);
            if (_storageFrame?.ParentFrame != null) _storageFrame = _storageFrame.ParentFrame;
            return (apiCalls, returnValue);
        }

        private void Execute(IList<Instruction> instructions, List<object>? parameters = null)
        {
            if (_storageFrame == null) throw new Exception("Frame is null!");

            var index = 0;
            while (index < instructions.Count)
            {
                var instr = instructions[index++];
                switch (instr.OpCode.Code)
                {
                    #region Loads and Stores
                    case Code.Ldarg_0:
                        _storageFrame.Stack.Push(parameters?.Count > 0 ? parameters[0] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Load argument 0 onto stack");
                        break;
                    case Code.Ldarg_1:
                        _storageFrame.Stack.Push(parameters?.Count > 1 ? parameters[1] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Load argument 1 onto stack");
                        break;
                    case Code.Ldarg_2:
                        _storageFrame.Stack.Push(parameters?.Count > 2 ? parameters[2] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Load argument 2 onto stack");
                        break;
                    case Code.Ldarg_3:
                        _storageFrame.Stack.Push(parameters?.Count > 3 ? parameters[3] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Load argument 3 onto stack");
                        break;
                    case Code.Ldarg:
                    case Code.Ldarg_S:
                        {
                            var operand = (Parameter)instr.Operand;
                            _storageFrame.Stack.Push(parameters?.Count > operand.Index ? parameters[operand.Index] : $"Dummy Value for '{instr.OpCode}'");
                            Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Load argument at index onto stack");
                            break;
                        }
                    case Code.Stloc_0:
                        HandleStoreLocal(0, index - 2 > 0 ? instructions[index - 2] : null);
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Pops a value from the stack into local variable 0");
                        break;
                    case Code.Stloc_1:
                        HandleStoreLocal(1, index - 2 > 0 ? instructions[index - 2] : null);
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Pops a value from the stack into local variable 1");
                        break;
                    case Code.Stloc_2:
                        HandleStoreLocal(2, index - 2 > 0 ? instructions[index - 2] : null);
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Pops a value from the stack into local variable 2");
                        break;
                    case Code.Stloc_3:
                        HandleStoreLocal(3, index - 2 > 0 ? instructions[index - 2] : null);
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Pops a value from the stack into local variable 3");
                        break;
                    case Code.Stloc:
                    case Code.Stloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            HandleStoreLocal(operand.Index, index - 2 > 0 ? instructions[index - 2] : null);
                            Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Pops a value from the stack and stores it in local variable index");
                            break;
                        }
                    case Code.Stfld:
                        {
                            var operand = (IField)instr.Operand;
                            var value = _storageFrame.Stack.Pop();
                            var obj = _storageFrame.Stack.Pop();

                            if (obj is GenericObj genericObj)
                            {
                                genericObj.Fields[operand.FullName] = value;
                                Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Stores value into field of {genericObj.GetType()}");
                            }
                            else
                            {
                                Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Stores value into field of Dummy");
                            }
                            break;
                        }
                    case Code.Ldloc_0:
                        _storageFrame.Stack.Push(_storageFrame.LocalVars[0]);
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Loads the local variable at index 0 onto the evaluation stack");
                        break;
                    case Code.Ldloc_1:
                        _storageFrame.Stack.Push(_storageFrame.LocalVars[1]);
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Loads the local variable at index 1 onto the evaluation stack");
                        break;
                    case Code.Ldloc_2:
                        _storageFrame.Stack.Push(_storageFrame.LocalVars[2]);
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Loads the local variable at index 2 onto the evaluation stack");
                        break;
                    case Code.Ldloc_3:
                        _storageFrame.Stack.Push(_storageFrame.LocalVars[3]);
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Loads the local variable at index 3 onto the evaluation stack");
                        break;
                    case Code.Ldloc:
                    case Code.Ldloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _storageFrame.Stack.Push(_storageFrame.LocalVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Loads the local variable at index index onto stack");
                            break;
                        }
                    case Code.Ldarga:
                    case Code.Ldarga_S:
                    case Code.Ldloca:
                    case Code.Ldloca_S:
                        {
                            // Normally the adress of a local variable/parameter would get pushed onto the stack.
                            // We are not interested in actual values. Therefore we just init the local variable directly.
                            var operand = (IVariable)instr.Operand;
                            _storageFrame.LocalVars[operand.Index] = $"Dummy Value for '{instr.OpCode}'";
                            _storageFrame.Stack.Push(_storageFrame.LocalVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Loads the address of the local variable/paramater at index onto the evaluation stack");
                            break;
                        }
                    case Code.Ldstr:
                        {
                            var operand = (string)instr.Operand;
                            _storageFrame.Stack.Push(operand);
                            Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} {operand} // Pushes a string object for the metadata string token mdToken");
                            break;
                        }
                    case Code.Ldfld:
                    case Code.Ldflda:
                        {
                            var operand = (IField)instr.Operand;
                            var obj = _storageFrame.Stack.Pop();

                            // only for fields set by stfld. For example Fields with default values etc. will not be set, because this vm doesn't "really" create valid objects/classes.
                            if (obj is GenericObj genericObj && genericObj.Fields.ContainsKey(operand.FullName))
                            {
                                _storageFrame.Stack.Push(genericObj.Fields[operand.FullName]);
                                Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} (Generic Object) // Pushes field value onto stack");
                            }
                            else
                            {
                                _storageFrame.Stack.Push($"Dummy Value for '{instr.OpCode}'");
                                Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} (Dummy) // Pushes field value onto stack");
                            }
                            break;
                        }
                    #endregion
                    #region Method Calls and Object Creation
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        index = HandleCall(instr, index, false);
                        break;
                    case Code.Newobj:
                        index = HandleCall(instr, index, true);
                        break;
                    #endregion
                    #region Branchings
                    case Code.Bgt:
                    case Code.Bgt_S:
                    case Code.Bgt_Un:
                    case Code.Bgt_Un_S:
                    case Code.Bge:
                    case Code.Bge_S:
                    case Code.Bge_Un:
                    case Code.Bge_Un_S:
                    case Code.Ble:
                    case Code.Ble_S:
                    case Code.Ble_Un:
                    case Code.Ble_Un_S:
                    case Code.Blt:
                    case Code.Blt_S:
                    case Code.Blt_Un:
                    case Code.Blt_Un_S:
                    case Code.Bne_Un:
                    case Code.Bne_Un_S:
                    case Code.Beq:
                    case Code.Beq_S:
                        _storageFrame.Stack.Pop();
                        _storageFrame.Stack.Pop();
                        break;
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        _storageFrame.Stack.Pop(); // pop the test value
                        break;
                    case Code.Br:
                    case Code.Br_S:
                        Debug.WriteLine($"[∙ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr}");
                        break;
                    #endregion
                    #region Stack Manipulations
                    case Code.Dup:
                        {
                            var obj = _storageFrame.Stack.Peek();
                            if (obj is IVMObj) _storageFrame.Stack.Push(obj);
                            else _storageFrame.Stack.Push($"Dummy Value for '{instr.OpCode}'");
                            Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Duplicates the value on the top of the stack.");
                            break;
                        }
                    case Code.Ldc_I4:   // Pushes the value num onto the stack.
                    case Code.Ldc_I4_0: // Pushes 0 onto the stack.
                    case Code.Ldc_I4_1: // Pushes 1 onto the stack.
                    case Code.Ldc_I4_2: // Pushes 2 onto the stack.
                    case Code.Ldc_I4_3: // Pushes 3 onto the stack.
                    case Code.Ldc_I4_4: // Pushes 4 onto the stack.
                    case Code.Ldc_I4_5: // Pushes 5 onto the stack.
                    case Code.Ldc_I4_6: // Pushes 6 onto the stack.
                    case Code.Ldc_I4_7: // Pushes 7 onto the stack.
                    case Code.Ldc_I4_8: // Pushes 8 onto the stack.
                    case Code.Ldc_I4_M1:// Pushes -1 onto the stack.
                    case Code.Ldc_I4_S: // Pushes num onto the stack as int32, short form.
                    case Code.Ldc_I8:   // Pushes num onto the stack as int64.
                    case Code.Ldc_R4:   // Pushes num onto the stack as F32.
                    case Code.Ldc_R8:   // Pushes num onto the stack as F64.
                    case Code.Ldnull:   // push a null reference onto the stack
                    case Code.Ldtoken:  // Converts a metadata token to its runtime representation.
                    case Code.Ldftn:    // Pushes a pointer to a method referenced by method on the stack.
                    case Code.Newarr:   // Creates a new array with elements of type etype.
                    case Code.Ldsfld:   // Push the value of static field on the stack.
                    case Code.Ldsflda:  // Push the adress of static field on the stack.
                        _storageFrame.Stack.Push($"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Push a value on the stack");
                        break;
                    case Code.Ceq:
                    case Code.Cgt:
                    case Code.Cgt_Un:
                    case Code.And:
                    case Code.Or:
                    case Code.Xor:
                    case Code.Sub:
                    case Code.Sub_Ovf:
                    case Code.Sub_Ovf_Un:
                    case Code.Div:
                    case Code.Div_Un:
                    case Code.Add:
                    case Code.Add_Ovf:
                    case Code.Add_Ovf_Un:
                    case Code.Ldelem:   // Loads the element at index onto the top of the stack as type typeTok.
                    case Code.Ldelema:
                    case Code.Ldelem_I:
                    case Code.Ldelem_I1:
                    case Code.Ldelem_I2:
                    case Code.Ldelem_I4:
                    case Code.Ldelem_I8:
                    case Code.Ldelem_R4:
                    case Code.Ldelem_R8:
                    case Code.Ldelem_Ref:
                    case Code.Ldelem_U1:
                    case Code.Ldelem_U2:
                    case Code.Ldelem_U4:
                        _storageFrame.Stack.Pop();
                        _storageFrame.Stack.Pop();
                        _storageFrame.Stack.Push($"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Uses two values and pushes result");
                        break;
                    case Code.Pop:
                    case Code.Stsfld:   // Stores value into static field
                    case Code.Throw:    // Throws an exception.
                        _storageFrame.Stack.Pop();
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr} // Pops a value for its operation");
                        break;
                    case Code.Stelem:       // Replaces the array element at the supplied index with a value of type typeTok on the stack.
                    case Code.Stelem_I:
                    case Code.Stelem_I2:
                    case Code.Stelem_I4:
                    case Code.Stelem_I8:
                    case Code.Stelem_R4:
                    case Code.Stelem_R8:
                    case Code.Stelem_Ref:
                        _storageFrame.Stack.Pop();
                        _storageFrame.Stack.Pop();
                        _storageFrame.Stack.Pop();
                        Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr}");
                        break;
                    #endregion
                    #region None Stack Changes
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Nop:
                    case Code.Castclass:
                    case Code.Box:
                    case Code.Isinst:
                    case Code.Initobj:
                    case Code.Ldlen:
                    case Code.Conv_I:
                    case Code.Conv_I1:
                    case Code.Conv_I2:
                    case Code.Conv_I4:
                    case Code.Conv_I8:
                    case Code.Conv_Ovf_I:
                    case Code.Conv_Ovf_I_Un:
                    case Code.Conv_Ovf_I1:
                    case Code.Conv_Ovf_I1_Un:
                    case Code.Conv_Ovf_I2:
                    case Code.Conv_Ovf_I2_Un:
                    case Code.Conv_Ovf_I4:
                    case Code.Conv_Ovf_I4_Un:
                    case Code.Conv_Ovf_I8:
                    case Code.Conv_Ovf_I8_Un:
                    case Code.Conv_Ovf_U:
                    case Code.Conv_Ovf_U_Un:
                    case Code.Conv_Ovf_U1:
                    case Code.Conv_Ovf_U1_Un:
                    case Code.Conv_Ovf_U2:
                    case Code.Conv_Ovf_U2_Un:
                    case Code.Conv_Ovf_U4:
                    case Code.Conv_Ovf_U4_Un:
                    case Code.Conv_Ovf_U8:
                    case Code.Conv_Ovf_U8_Un:
                        Debug.WriteLine($"[∙ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr}");
                        break;
                    #endregion
                    case Code.Ret:
                        Debug.WriteLine($"[∙ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] {instr}");
                        index = instructions.Count; // break loop
                        break;
                    default:
                        Debug.WriteLine($"NOT IMPLEMENTED {instr.OpCode}");
                        break;
                }
            }
        }

        private void HandleStoreLocal(int varIndex, Instruction? previousInstr = null)
        {
            if (_storageFrame.Stack.Count == 0 && previousInstr?.IsLeave() == true)
                _storageFrame.LocalVars[varIndex] = $"Propably dummy exception";
            else if (_storageFrame.Stack.Peek() is GenericObj vmObj && vmObj.IsRecursiveReturnValue)
            {
                _storageFrame.Stack.Pop(); // Don't overwrite the saved value with the recursive one
                if (_storageFrame.LocalVars[varIndex] is EntityObj eObj) eObj.CallLoopHit = true; // Set Info on Entity, that there are uncertainties because of the recursion
            }
            else
                _storageFrame.LocalVars[varIndex] = _storageFrame.Stack.Pop();
        }

        private int HandleCall(Instruction instr, int index, bool isNewObj = false)
        {
            var (method, methodDef, parameters) = GetMethodInfos(instr, isNewObj);
            Debug.WriteLine($"CALL {instr.ToString()}]");

            var hookExecuted = false;
            foreach (var hook in _callHooks)
            {
                if (hook.HookApplicable(method, methodDef, parameters))
                {
                    var apiCall = hook.ExecuteHook(method, methodDef, parameters, _storageFrame.Stack);
                    if (apiCall != null) _storageFrame.ApiCalls.Add(apiCall);

                    hookExecuted = true;
                    Debug.WriteLine($"[∙ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] Hook executed!");
                    break;
                }
            }

            if (!hookExecuted)
            {
                // The called method is part of the loaded dlls
                // Interpret it to get more call information
                // Check for possible call loops (recursions)
                var callLoopHit = methodDef != null && IsCallLoop(methodDef.FullName);
                if (methodDef != null && methodDef.Body != null && !callLoopHit)
                {
                    _storageFrame.CallStack.Push(methodDef.FullName);
                    var vm = new PluginGraphVM(_moduleList);
                    var (apicalls, returnValue) = vm.Execute(methodDef, parameters, _storageFrame);
                    _storageFrame.CallStack.Pop();

                    _storageFrame.ApiCalls.AddRange(apicalls);

                    if (returnValue != null)
                    {
                        _storageFrame.Stack.Push(returnValue);
                        Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] Return value from {methodDef.FullName}");
                    }
                }
                else if (method.MethodSig.RetType.FullName != "System.Void")
                {
                    _storageFrame.Stack.Push(new GenericObj($"Dummy return value for '{method.FullName}'", callLoopHit));
                    Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] Return value from {method.FullName}");
                }

                if (isNewObj)
                {
                    _storageFrame.Stack.Push(((GenericObj)parameters[0]).GetObject());
                    if (_storageFrame.Stack.Peek() is XrmApiCall apiCall) _storageFrame.ApiCalls.Add(apiCall);
                    Debug.WriteLine($"[↑ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] New object {_storageFrame.Stack.Peek().GetType()} created by {method.FullName}");
                }
            }
            Debug.WriteLine($"ENDCALL {instr.ToString()}]");
            return index;
        }

        private (IMethod, MethodDef?, List<object>) GetMethodInfos(Instruction instr, bool isNewObj)
        {
            var method = (IMethod)instr.Operand;
            var reflectionTypeName = method.DeclaringType.ScopeType.FullName;
            var methodDef = _moduleList.TryFindMethod(reflectionTypeName, method.FullName.Replace(method.DeclaringType.FullName, reflectionTypeName));

            // Get all parameters from the current stack
            var parameters = new List<object>();
            for (var i = 0; i < method.MethodSig.GetParamCount(); i++) parameters.Add(_storageFrame.Stack.Pop());

            GenericObj newObj = null;
            // If not a static call, pop 'this' from the stack
            if (!isNewObj && method.MethodSig.HasThis) parameters.Add(_storageFrame.Stack.Pop());
            // If NewObj, Create a object and add as 'this'
            if (isNewObj)
            {
                newObj = new GenericObj($"New Object ({method.FullName})");
                parameters.Add(newObj);
            }

            // Reverse List to have the parameters in order: this, param1, param2 ....
            parameters.Reverse();
            if (parameters.Any()) Debug.WriteLine($"[↓ {_storageFrame.Stack.Count}][{_storageFrame.CallStack.Count}] Popped method arguments");


            // If the method is abstract, we have to search the method in the
            // declaring parent type (parameter on index 0).
            methodDef = methodDef != null && methodDef.IsAbstract && parameters.Any() && parameters[0] is TypeDef typeDef
                ? _moduleList.TryFindMethod(typeDef.FullName, method.FullName.Replace(method.DeclaringType.FullName, typeDef.FullName))
                : methodDef;

            return (method, methodDef, parameters);
        }

        private bool IsCallLoop(string methodFullname)
        {
            var loopDetected = false;
            // Add method name temporally onto the callstack
            _storageFrame.CallStack.Push(methodFullname);

            for (int i = 0; i < 4; i++)
            {
                if (_storageFrame.CallStack.Count % (i + 1) != 0 || _storageFrame.CallStack.Count / (i + 1) < 2) continue;
                loopDetected = Enumerable.Range(0, _storageFrame.CallStack.Count - 1).All(j => _storageFrame.CallStack.ElementAt(j) == _storageFrame.CallStack.ElementAt(j + i + 1));
                if (loopDetected) break;
            }

            // remove the method from the callstack
            _storageFrame.CallStack.Pop();
            return loopDetected;
        }
    }
}
