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

        private readonly Stack<string> _callStack;
        private readonly List<XrmApiCall> _apiCalls = new();
        private readonly object[] _localVars = new object[255];

        private MethodDef? _methodDef;
        private Stack<object> _stack = new();
        private bool _callLoopHit = false;

        public PluginGraphVM(PluginModuleList moduleList, Stack<string>? callStack = null)
        {
            _moduleList = moduleList;
            _callStack = callStack ?? new();

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
        public (List<XrmApiCall>, object?) Execute(MethodDef methodDef, List<object>? parameters = null)
        {
            _methodDef = methodDef;
            Execute(methodDef.Body.Instructions, parameters);
            return (_apiCalls, _stack.Count > 0 ? _stack.Pop() : null);
        }

        private void Execute(IList<Instruction> instructions, List<object>? parameters = null)
        {
            var index = 0;
            while (index < instructions.Count)
            {
                var instr = instructions[index++];
                switch (instr.OpCode.Code)
                {
                    #region Loads and Stores
                    case Code.Ldarg_0:
                        _stack.Push(parameters?.Count > 0 ? parameters[0] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Load argument 0 onto stack");
                        break;
                    case Code.Ldarg_1:
                        _stack.Push(parameters?.Count > 1 ? parameters[1] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Load argument 1 onto stack");
                        break;
                    case Code.Ldarg_2:
                        _stack.Push(parameters?.Count > 2 ? parameters[2] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Load argument 2 onto stack");
                        break;
                    case Code.Ldarg_3:
                        _stack.Push(parameters?.Count > 3 ? parameters[3] : $"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Load argument 3 onto stack");
                        break;
                    case Code.Ldarg:
                    case Code.Ldarg_S:
                        {
                            var operand = (Parameter)instr.Operand;
                            _stack.Push(parameters?.Count > operand.Index ? parameters[operand.Index] : $"Dummy Value for '{instr.OpCode}'");
                            Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Load argument at index onto stack");
                            break;
                        }
                    case Code.Stloc_0:
                        _localVars[0] = (_stack.Count == 0 && instructions[index - 2].IsLeave()) ? $"Propably dummy exception for '{instr.OpCode}'" : _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Pops a value from the stack into local variable 0");
                        break;
                    case Code.Stloc_1:
                        _localVars[1] = (_stack.Count == 0 && instructions[index - 2].IsLeave()) ? $"Propably dummy exception for '{instr.OpCode}'" : _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Pops a value from the stack into local variable 1");
                        break;
                    case Code.Stloc_2:
                        _localVars[2] = (_stack.Count == 0 && instructions[index - 2].IsLeave()) ? $"Propably dummy exception for '{instr.OpCode}'" : _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Pops a value from the stack into local variable 2");
                        break;
                    case Code.Stloc_3:
                        _localVars[3] = (_stack.Count == 0 && instructions[index - 2].IsLeave()) ? $"Propably dummy exception for '{instr.OpCode}'" : _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Pops a value from the stack into local variable 3");
                        break;
                    case Code.Stloc:
                    case Code.Stloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _localVars[operand.Index] = (_stack.Count == 0 && instructions[index - 2].IsLeave()) ? $"Propably dummy exception for '{instr.OpCode}'" : _stack.Pop();
                            Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Pops a value from the stack and stores it in local variable index");
                            break;
                        }
                    case Code.Stfld:
                        {
                            var operand = (IField)instr.Operand;
                            var value = _stack.Pop();
                            var obj = _stack.Pop();

                            if (obj is GenericObj genericObj)
                            {
                                genericObj.Fields[operand.FullName] = value;
                                Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Stores value into field of {genericObj.GetType()}");
                            }
                            else
                            {
                                Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Stores value into field of Dummy");
                            }
                            break;
                        }
                    case Code.Ldloc_0:
                        _stack.Push(_localVars[0]);
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Loads the local variable at index 0 onto the evaluation stack");
                        break;
                    case Code.Ldloc_1:
                        _stack.Push(_localVars[1]);
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Loads the local variable at index 1 onto the evaluation stack");
                        break;
                    case Code.Ldloc_2:
                        _stack.Push(_localVars[2]);
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Loads the local variable at index 2 onto the evaluation stack");
                        break;
                    case Code.Ldloc_3:
                        _stack.Push(_localVars[3]);
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Loads the local variable at index 3 onto the evaluation stack");
                        break;
                    case Code.Ldloc:
                    case Code.Ldloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _stack.Push(_localVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Loads the local variable at index index onto stack");
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
                            _localVars[operand.Index] = $"Dummy Value for '{instr.OpCode}'";
                            _stack.Push(_localVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Loads the address of the local variable/paramater at index onto the evaluation stack");
                            break;
                        }
                    case Code.Ldstr:
                        {
                            var operand = (string)instr.Operand;
                            _stack.Push(operand);
                            Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} {operand} // Pushes a string object for the metadata string token mdToken");
                            break;
                        }
                    case Code.Ldfld:
                    case Code.Ldflda:
                        {
                            var operand = (IField)instr.Operand;
                            var obj = _stack.Pop();

                            // only for fields set by stfld. For example Fields with default values etc. will not be set, because this vm doesn't "really" create valid objects/classes.
                            if (obj is GenericObj genericObj && genericObj.Fields.ContainsKey(operand.FullName))
                            {
                                _stack.Push(genericObj.Fields[operand.FullName]);
                                Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} (Generic Object) // Pushes field value onto stack");
                            }
                            else
                            {
                                _stack.Push($"Dummy Value for '{instr.OpCode}'");
                                Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} (Dummy) // Pushes field value onto stack");
                            }
                            break;
                        }
                    #endregion
                    #region Method Calls and Object Creation
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        HandleCall(instr, false);
                        break;
                    case Code.Newobj:
                        HandleCall(instr, true);
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
                        {
                            // TODO
                            _stack.Pop();
                            _stack.Pop();
                            Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Acts based on two values");
                            break;
                        }
                    case Code.Leave:
                    case Code.Leave_S:
                        {
                            //if (branched) return; // A Leave Operation in a branching execution will be the as returning and continue previous method execution in our case
                            //index = ExecuteBranch(instructions, parameters, index, instr);
                            break;
                        }
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        {
                            _stack.Pop(); // pop the test value
                            index = HandleBranch(instructions, parameters, index, instr);
                            break;
                        }
                    case Code.Br:
                    case Code.Br_S:
                        {
                            var operand = (Instruction)instr.Operand;
                            while (index < instructions.Count && instructions[index].Offset != operand.Offset) index++;
                            Debug.WriteLine($"[∙ {_stack.Count}][{_callStack.Count}] {instr}");
                            break;
                        }
                    #endregion
                    #region Stack Manipulations
                    case Code.Dup:
                        {
                            var obj = _stack.Peek();
                            if (obj is IVMObj) _stack.Push(obj);
                            else _stack.Push($"Dummy Value for '{instr.OpCode}'");
                            Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Duplicates the value on the top of the stack.");
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
                        _stack.Push($"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] {instr} // Push a value on the stack");
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
                        _stack.Pop();
                        _stack.Pop();
                        _stack.Push($"Dummy Value for '{instr.OpCode}'");
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Uses two values and pushes result");
                        break;
                    case Code.Pop:
                    case Code.Stsfld:   // Stores value into static field
                    case Code.Throw:    // Throws an exception.
                        _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr} // Pops a value for its operation");
                        break;
                    case Code.Stelem:       // Replaces the array element at the supplied index with a value of type typeTok on the stack.
                    case Code.Stelem_I:
                    case Code.Stelem_I2:
                    case Code.Stelem_I4:
                    case Code.Stelem_I8:
                    case Code.Stelem_R4:
                    case Code.Stelem_R8:
                    case Code.Stelem_Ref:
                        _stack.Pop();
                        _stack.Pop();
                        _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] {instr}");
                        break;
                    #endregion
                    #region None Stack Changes
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
                        Debug.WriteLine($"[∙ {_stack.Count}][{_callStack.Count}] {instr}");
                        break;
                    #endregion
                    case Code.Ret:
                        Debug.WriteLine($"[∙ {_stack.Count}][{_callStack.Count}] {instr}");
                        index = instructions.Count; // break loop
                        break;
                    default:
                        Debug.WriteLine($"NOT IMPLEMENTED {instr.OpCode}");
                        break;
                }
            }
        }

        private int HandleBranch(IList<Instruction> instructions, List<object>? parameters, int index, Instruction instr)
        {
            var operand = (Instruction)instr.Operand;
            var blockInstructions = instructions
                    .Where(i => i.Offset > instr.Offset && i.Offset < operand.Offset)
                    .ToList();
            var remainingInstructions = instructions
                .Where(i => i.Offset >= operand.Offset)
                .ToList();

            // Both blocks get checked for possible call loops by evaluating them
            // If one of them contains a call loop, the other block gets choosen for the remaining main execution
            var checkBlock = (List<Instruction> instructions) =>
            {
                Debug.WriteLine($"BRANCH [Index: {index}, Instruction Set: {instructions.Count}, Instr: {instr}, Containing Method: {_methodDef.FullName}]");

                var savedStack = new Stack<object>(_stack);
                Execute(instructions, parameters);
                _stack = savedStack;

                var returnValue = _callLoopHit;
                if (_callLoopHit)
                {
                    Debug.WriteLine($"CALL LOOP DETECTED!");
                    _callLoopHit = false;
                }
                Debug.WriteLine($"ENDBRANCH");
                return returnValue;
            };

            var isBlockLoop = checkBlock(blockInstructions);
            checkBlock(remainingInstructions);

            // If the block instructions include a call loop,
            // the main execution skippes to the end of the block 
            // That way the block got evaluated by the 'checkBlock' above for any entity/api calls
            // but we continue the main execution for the stack tracing on the remaining instructions to skip the loop.
            // Otherwise, we return the current index to continue the main execution on the block instructions and will skip the instuctions after the block (already evaluated by the checkBlock call above).
            if (isBlockLoop)
            {
                while (instructions[index].Offset != operand.Offset) index++;
            }

            return index;
        }

        private void HandleCall(Instruction instr, bool isNewObj = false)
        {
            var (method, methodDef, parameters) = GetMethodInfos(instr, isNewObj);
            Debug.WriteLine($"CALL {instr.ToString()}]");

            var hookExecuted = false;
            foreach (var hook in _callHooks)
            {
                if (hook.HookApplicable(method, methodDef, parameters))
                {
                    var apiCall = hook.ExecuteHook(method, methodDef, parameters, ref _stack);
                    if (apiCall != null) _apiCalls.Add(apiCall);

                    hookExecuted = true;
                    Debug.WriteLine($"[∙ {_stack.Count}][{_callStack.Count}] Hook executed!");
                    break;
                }
            }

            if (!hookExecuted)
            {
                // The called method is part of the loaded dlls
                // Interpret it to get more call information
                // Check for possible call loops (recursions)
                if (methodDef != null && methodDef.Body != null && !IsCallLoop(methodDef.FullName))
                {
                    _callStack.Push(methodDef.FullName);
                    var vm = new PluginGraphVM(_moduleList, _callStack);
                    var (apicalls, returnValue) = vm.Execute(methodDef, parameters);
                    _callStack.Pop();

                    _apiCalls.AddRange(apicalls);

                    if (returnValue != null)
                    {
                        _stack.Push(returnValue);
                        Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] Return value from {methodDef.FullName}");
                    }
                }
                else if (method.MethodSig.RetType.FullName != "System.Void")
                {
                    _stack.Push(new GenericObj($"Dummy return value for '{method.FullName}'"));
                    Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] Return value from {method.FullName}");
                }

                if (isNewObj)
                {
                    _stack.Push(((GenericObj)parameters[0]).GetObject());
                    if (_stack.Peek() is XrmApiCall apiCall) _apiCalls.Add(apiCall);
                    Debug.WriteLine($"[↑ {_stack.Count}][{_callStack.Count}] New object {_stack.Peek().GetType()} created by {method.FullName}");
                }
            }
            Debug.WriteLine($"ENDCALL {instr.ToString()}]");
        }

        private (IMethod, MethodDef?, List<object>) GetMethodInfos(Instruction instr, bool isNewObj)
        {
            var method = (IMethod)instr.Operand;
            var reflectionTypeName = method.DeclaringType.ScopeType.FullName;
            var methodDef = _moduleList.TryFindMethod(reflectionTypeName, method.FullName.Replace(method.DeclaringType.FullName, reflectionTypeName));

            // Get all parameters from the current stack
            var parameters = new List<object>();
            for (var i = 0; i < method.MethodSig.GetParamCount(); i++) parameters.Add(_stack.Pop());

            GenericObj newObj = null;
            // If not a static call, pop 'this' from the stack
            if (!isNewObj && method.MethodSig.HasThis) parameters.Add(_stack.Pop());
            // If NewObj, Create a object and add as 'this'
            if (isNewObj)
            {
                newObj = new GenericObj($"New Object ({method.FullName})");
                parameters.Add(newObj);
            }

            // Reverse List to have the parameters in order: this, param1, param2 ....
            parameters.Reverse();
            if (parameters.Any()) Debug.WriteLine($"[↓ {_stack.Count}][{_callStack.Count}] Popped method arguments");


            // If the method is abstract, we have to search the method in the
            // declaring parent type (parameter on index 0).
            methodDef = methodDef != null && methodDef.IsAbstract && parameters.Any() && parameters[0] is TypeDef typeDef
                ? _moduleList.TryFindMethod(typeDef.FullName, method.FullName.Replace(method.DeclaringType.FullName, typeDef.FullName))
                : methodDef;

            return (method, methodDef, parameters);
        }

        public bool IsCallLoop(string methodFullname)
        {
            var loopDetected = false;
            // Add method name temporally onto the callstack
            _callStack.Push(methodFullname);

            for (int i = 0; i < 4; i++)
            {
                if (_callStack.Count % (i + 1) != 0 || _callStack.Count / (i + 1) < 2) continue;
                loopDetected = Enumerable.Range(0, _callStack.Count - 1).All(j => _callStack.ElementAt(j) == _callStack.ElementAt(j + i + 1));
                if (loopDetected)
                {
                    _callLoopHit = loopDetected;
                    break;
                }
            }

            // remove the method from the callstack
            _callStack.Pop();
            return loopDetected;
        }
    }
}
