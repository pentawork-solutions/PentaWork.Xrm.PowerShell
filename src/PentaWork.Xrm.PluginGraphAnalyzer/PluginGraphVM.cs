using dnlib.DotNet;
using dnlib.DotNet.Emit;
using PentaWork.Xrm.PluginGraph.Model;
using System.Diagnostics;

namespace PentaWork.Xrm.PluginGraph
{
    public class PluginGraphVM
    {
        private readonly PluginModuleList _moduleList;

        private readonly Stack<object> _stack = new();
        private readonly object[] _localVars = new object[255];

        private readonly List<XrmApiCall> _apiCalls = new();

        public PluginGraphVM(PluginModuleList moduleList)
        {
            _moduleList = moduleList;
        }

        public (List<XrmApiCall>, object?) Execute(IList<Instruction> instructions, List<object>? parameters = null)
        {
            var index = 0;
            while (index < instructions.Count)
            {
                var instr = instructions[index++];
                switch (instr.OpCode.Code)
                {
                    case Code.Ldarg_0:
                        _stack.Push(parameters?.Count > 0 ? parameters[0] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldarg_1:
                        _stack.Push(parameters?.Count > 1 ? parameters[1] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldarg_2:
                        _stack.Push(parameters?.Count > 2 ? parameters[2] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldarg_3:
                        _stack.Push(parameters?.Count > 3 ? parameters[3] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Stloc_0:
                        _localVars[0] = _stack.Pop();
                        Debug.WriteLine($"DECREASE [{_stack.Count}] and store ({instr.ToString()})");
                        break;
                    case Code.Stloc_1:
                        _localVars[1] = _stack.Pop();
                        Debug.WriteLine($"DECREASE [{_stack.Count}] and store ({instr.ToString()})");
                        break;
                    case Code.Stloc_2:
                        _localVars[2] = _stack.Pop();
                        Debug.WriteLine($"DECREASE [{_stack.Count}] and store ({instr.ToString()})");
                        break;
                    case Code.Stloc_3:
                        _localVars[3] = _stack.Pop();
                        Debug.WriteLine($"DECREASE [{_stack.Count}] and store ({instr.ToString()})");
                        break;
                    case Code.Stloc:
                    case Code.Stloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _localVars[operand.Index] = _stack.Pop();
                            Debug.WriteLine($"DECREASE [{_stack.Count}] and store ({instr.ToString()})");
                            break;
                        }
                    case Code.Ldloc_0:
                        _stack.Push(_localVars[0]);
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldloc_1:
                        _stack.Push(_localVars[1]);
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldloc_2:
                        _stack.Push(_localVars[2]);
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldloc_3:
                        _stack.Push(_localVars[3]);
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldloc:
                    case Code.Ldloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _stack.Push(_localVars[operand.Index]);
                            Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                            break;
                        }
                    case Code.Ldloca:
                    case Code.Ldloca_S:
                        {
                            // will be used as a reference - therefore we will just init the variable space as a dummy
                            var operand = (Local)instr.Operand;
                            _localVars[operand.Index] = $"Dummy Value for '{instr.OpCode.ToString()}'";
                            _stack.Push(_localVars[operand.Index]);
                            Debug.WriteLine($"Load local variable address [{_stack.Count}] ({instr.ToString()})");
                            break;
                        }
                    case Code.Ldstr:
                        {
                            var operand = (string)instr.Operand;
                            _stack.Push(operand);
                            Debug.WriteLine($"INCREASE [{_stack.Count}] (Load string: {operand})");
                            break;
                        }
                    case Code.Newobj:
                        HandleObjectCreation(instr);
                        Debug.WriteLine($"New Object [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        HandleCall(instr);
                        Debug.WriteLine($"Call [{_stack.Count}] ({instr.ToString()})");
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
                    case Code.Dup:
                        _stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"INCREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ldfld:
                        _stack.Pop();
                        _stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"CHANGE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Ceq:
                    case Code.Cgt:
                    case Code.Cgt_Un:
                    case Code.And:
                    case Code.Or:
                    case Code.Xor:
                        _stack.Pop();
                        _stack.Pop();
                        _stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"DECREASE [{_stack.Count}] ({instr.ToString()})");
                        break;
                    case Code.Stfld:
                    case Code.Beq:
                    case Code.Beq_S:
                        _stack.Pop();
                        _stack.Pop();
                        break;
                    case Code.Pop:
                    case Code.Stsfld:
                    case Code.Throw:
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        _stack.Pop();
                        Debug.WriteLine($"DECREASE [{_stack.Count}]({instr.ToString()})");
                        break;
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Br:
                    case Code.Br_S:
                        {
                            var operand = (Instruction)instr.Operand;
                            while (instructions[index].Offset != operand.Offset) index++;
                            break;
                        }
                    #endregion
                    default:
                        Debug.WriteLine($"No stack change ({instr.OpCode.ToString()})");
                        break;
                }
            }

            return (_apiCalls, _stack.Count > 0 ? _stack.Pop() : null);
        }

        private void HandleObjectCreation(Instruction instr)
        {
            var method = (IMethod)instr.Operand;
            var methodDef = _moduleList.TryFindMethod(method.DeclaringType.FullName, method.FullName);

            // Entity Creation
            if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Entity::.ctor(System.String)")
            {
                var entity = new EntityObj();
                entity.LogicalName = (string)_stack.Pop();
                _stack.Push(entity);
            }
            // Maybe a proxy class creation
            else if (methodDef != null && methodDef.DeclaringType.BaseType?.FullName == "Microsoft.Xrm.Sdk.Entity")
            {
                Debug.WriteLine($"[Start creating {methodDef.FullName}]");

                var entity = new EntityObj();
                var vm = new PluginGraphVM(_moduleList);
                var (apicalls, returnValue) = vm.Execute(methodDef.Body.Instructions, [entity]);

                _stack.Push(entity);
                _apiCalls.AddRange(apicalls);
                if (returnValue != null) _stack.Push(returnValue);

                Debug.WriteLine($"[End creating {methodDef.FullName}]");
            }
            else
            {
                // get the constructor parameters from the stack
                var parameters = new Stack<object>();
                for (var i = 0; i < method.MethodSig.GetParamCount(); i++) parameters.Push(_stack.Pop());

                _stack.Push($"New Object ({method.FullName})");
            }
        }

        private void HandleCall(Instruction instr)
        {
            var method = (IMethod)instr.Operand;
            var methodDef = _moduleList.TryFindMethod(method.DeclaringType.FullName, method.FullName);

            // Get all parameters from the current stack
            var parameters = new List<object>();
            for (var i = 0; i < method.MethodSig.GetParamCount(); i++) parameters.Add(_stack.Pop());

            // If not a static call, pop 'this' from the stack
            if (method.MethodSig.HasThis) parameters.Add(_stack.Pop());

            // Reverse List to have the parameters in order: this, param1, param2 ....
            parameters.Reverse();

            if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Entity::.ctor(System.String)")
            {
                var entity = (EntityObj)parameters[0];
                entity.LogicalName = (string)parameters[1];
            }
            else if (method.FullName == "System.Guid Microsoft.Xrm.Sdk.IOrganizationService::Create(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "create";
                apiCall.EntityInfo = (EntityObj)parameters[1];

                _apiCalls.Add(apiCall);

                _stack.Push($"Dummy return value for '{method.FullName}'");
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Update(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "update";
                apiCall.EntityInfo = (EntityObj)parameters[1];

                _apiCalls.Add(apiCall);
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.IOrganizationService::Delete(System.String,System.Guid)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "delete";
                apiCall.EntityInfo = new EntityObj();
                apiCall.EntityInfo.LogicalName = (string)parameters[1];

                _apiCalls.Add(apiCall);

                _stack.Push($"Dummy return value for '{method.FullName}'");
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::AddObject(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "create";
                apiCall.EntityInfo = (EntityObj)parameters[1];
                apiCall.IsExecuted = false;

                _apiCalls.Add(apiCall);
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::UpdateObject(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "update";
                apiCall.EntityInfo = (EntityObj)parameters[1];
                apiCall.IsExecuted = false;

                _apiCalls.Add(apiCall);
            }
            else if (method.FullName == "Microsoft.Xrm.Sdk.SaveChangesResultCollection Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::SaveChanges()")
            {
                foreach (var apiCall in _apiCalls.Where(a => !a.IsExecuted))
                {
                    apiCall.IsExecuted = true;
                }
                _stack.Push($"Dummy return value for '{method.FullName}'");
            }
            // Add the entity object to the top of the stack
            else if (method.FullName == "Microsoft.Xrm.Sdk.AttributeCollection Microsoft.Xrm.Sdk.Entity::get_Attributes()")
            {
                _stack.Push(parameters[0]);
            }
            // The entity object should be on top of the stack and we are able to add the used fields
            // Parameters Stack Top to Bottom => Entity, Field Name, Field Value
            else if (method.FullName is
                "System.Void Microsoft.Xrm.Sdk.Entity::SetAttributeValue(System.String,System.Object)" or
                "System.Void Microsoft.Xrm.Sdk.DataCollection`2<System.String,System.Object>::set_Item(System.String,System.Object)")
            {
                var entity = (EntityObj)parameters[0];
                entity.UsedFields.Add((string)parameters[1]);
            }
            // The called method is part of the plugin dlls
            // Interpret it to get more call information
            else if (methodDef != null && methodDef.Body != null)
            {
                Debug.WriteLine($"[Start calling {methodDef.FullName}]");

                var vm = new PluginGraphVM(_moduleList);
                var (apicalls, returnValue) = vm.Execute(methodDef.Body.Instructions, parameters);

                _apiCalls.AddRange(apicalls);
                if (returnValue != null)
                {
                    _stack.Push(returnValue);
                    Debug.WriteLine($"INCREASE [{_stack.Count}] Return Value");
                }
                Debug.WriteLine($"[End calling {methodDef.FullName}]");
            }
            else
            {
                if (method.MethodSig.RetType.FullName != "System.Void") _stack.Push($"Dummy return value for '{method.FullName}'");
            }
        }
    }
}
