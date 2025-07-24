using dnlib.DotNet;
using dnlib.DotNet.Emit;
using PentaWork.Xrm.PluginGraph.Model;
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
        private readonly PluginModuleList _moduleList;

        private readonly Stack<object> _stack = new();
        private readonly object[] _localVars = new object[255];

        private readonly List<XrmApiCall> _apiCalls = new();

        public PluginGraphVM(PluginModuleList moduleList)
        {
            _moduleList = moduleList;
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
                        _stack.Push(parameters?.Count > 0 ? parameters[0] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Load argument 0 onto stack");
                        break;
                    case Code.Ldarg_1:
                        _stack.Push(parameters?.Count > 1 ? parameters[1] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Load argument 1 onto stack");
                        break;
                    case Code.Ldarg_2:
                        _stack.Push(parameters?.Count > 2 ? parameters[2] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Load argument 2 onto stack");
                        break;
                    case Code.Ldarg_3:
                        _stack.Push(parameters?.Count > 3 ? parameters[3] : $"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Load argument 3 onto stack");
                        break;
                    case Code.Stloc_0:
                        _localVars[0] = _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 0");
                        break;
                    case Code.Stloc_1:
                        _localVars[1] = _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 1");
                        break;
                    case Code.Stloc_2:
                        _localVars[2] = _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 2");
                        break;
                    case Code.Stloc_3:
                        _localVars[3] = _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Pops a value from the stack into local variable 3");
                        break;
                    case Code.Stloc:
                    case Code.Stloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _localVars[operand.Index] = _stack.Pop();
                            Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Pops a value from the stack and stores it in local variable index");
                            break;
                        }
                    case Code.Ldloc_0:
                        _stack.Push(_localVars[0]);
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Loads the local variable at index 0 onto the evaluation stack");
                        break;
                    case Code.Ldloc_1:
                        _stack.Push(_localVars[1]);
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Loads the local variable at index 1 onto the evaluation stack");
                        break;
                    case Code.Ldloc_2:
                        _stack.Push(_localVars[2]);
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Loads the local variable at index 2 onto the evaluation stack");
                        break;
                    case Code.Ldloc_3:
                        _stack.Push(_localVars[3]);
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Loads the local variable at index 3 onto the evaluation stack");
                        break;
                    case Code.Ldloc:
                    case Code.Ldloc_S:
                        {
                            var operand = (Local)instr.Operand;
                            _stack.Push(_localVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Loads the local variable at index index onto stack");
                            break;
                        }
                    case Code.Ldloca:
                    case Code.Ldloca_S:
                        {
                            // Normally the adress of a local variable would get pushed onto the stack.
                            // We are not interested in actual values. Therefore we just init the local variable directly.
                            var operand = (Local)instr.Operand;
                            _localVars[operand.Index] = $"Dummy Value for '{instr.OpCode.ToString()}'";
                            _stack.Push(_localVars[operand.Index]);
                            Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Loads the address of the local variable at index onto the evaluation stack");
                            break;
                        }
                    case Code.Ldstr:
                        {
                            var operand = (string)instr.Operand;
                            _stack.Push(operand);
                            Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} {operand} // Pushes a string object for the metadata string token mdToken");
                            break;
                        }
                    case Code.Newobj:
                        Debug.WriteLine($"- [Start {instr.ToString()}]");
                        HandleObjectCreation(instr);
                        Debug.WriteLine($"- [End {instr.ToString()}]");
                        break;
                    case Code.Call:
                    case Code.Calli:
                    case Code.Callvirt:
                        Debug.WriteLine($"- [Start {instr.ToString()}]");
                        HandleCall(instr);
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
                    case Code.Dup:
                        _stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[↑ {_stack.Count}] {instr.ToString()} // Push a value on the stack");
                        break;
                    case Code.Ldfld:
                        _stack.Pop();
                        _stack.Push($"Dummy Value for '{instr.OpCode.ToString()}'");
                        Debug.WriteLine($"[∙ {_stack.Count}] {instr.ToString()} // Pushes the value of a field in a specified object onto the stack (pops the object reference during operation)");
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
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Compares two values and pushes result");
                        break;
                    case Code.Stfld:
                    case Code.Beq:
                    case Code.Beq_S:
                        _stack.Pop();
                        _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Acts based on two values");
                        break;
                    case Code.Pop:
                    case Code.Stsfld:
                    case Code.Throw:
                    case Code.Brfalse:
                    case Code.Brfalse_S:
                    case Code.Brtrue:
                    case Code.Brtrue_S:
                        _stack.Pop();
                        Debug.WriteLine($"[↓ {_stack.Count}] {instr.ToString()} // Pops a value for its operation");
                        break;
                    case Code.Leave:
                    case Code.Leave_S:
                    case Code.Br:
                    case Code.Br_S:
                        {
                            var operand = (Instruction)instr.Operand;
                            while (instructions[index].Offset != operand.Offset) index++;
                            Debug.WriteLine($"[∙ {_stack.Count}] {instr.ToString()} // Jumps to another code part");
                            break;
                        }
                    #endregion
                    case Code.Nop:
                    case Code.Ret:
                    case Code.Castclass:
                        Debug.WriteLine($"[∙ {_stack.Count}] {instr.ToString()}");
                        break;
                    default:
                        Debug.WriteLine($"NOT IMPLEMENTED {instr.OpCode.ToString()}");
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
                Debug.WriteLine($"[↑ {_stack.Count}] {method.FullName}");
            }
            // Maybe a proxy class creation
            else if (methodDef != null && methodDef.DeclaringType.BaseType?.FullName == "Microsoft.Xrm.Sdk.Entity")
            {
                var entity = new EntityObj();
                var vm = new PluginGraphVM(_moduleList);
                var (apicalls, returnValue) = vm.Execute(methodDef, [entity]);

                _stack.Push(entity);
                _apiCalls.AddRange(apicalls);
                if (returnValue != null)
                {
                    _stack.Push(returnValue);
                    Debug.WriteLine($"[↑ {_stack.Count}] {method.FullName}");
                }
            }
            // ServiceContext Creation
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::.ctor(Microsoft.Xrm.Sdk.IOrganizationService)")
            {
                var serviceContext = new ServiceContextObj();
                _stack.Push(serviceContext);
                Debug.WriteLine($"[↑ {_stack.Count}] {method.FullName}");
            }
            else
            {
                // get the constructor parameters from the stack
                var parameters = new Stack<object>();
                for (var i = 0; i < method.MethodSig.GetParamCount(); i++) parameters.Push(_stack.Pop());

                _stack.Push($"New Object ({method.FullName})");
                Debug.WriteLine($"[↑ {_stack.Count}] {method.FullName}");
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
            if (parameters.Any()) Debug.WriteLine($"[↓ {_stack.Count}] Popped method arguments");

            // Base Constructor Call, if proxies are used
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
                Debug.WriteLine($"[↑ {_stack.Count}] Return value from {method.FullName}");
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
                Debug.WriteLine($"[↑ {_stack.Count}] Return value from {method.FullName}");
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::AddObject(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "create";
                apiCall.EntityInfo = (EntityObj)parameters[1];
                apiCall.IsExecuted = false;

                _apiCalls.Add(apiCall);

                var serviceContext = (ServiceContextObj)parameters[0];
                serviceContext.AddCall(apiCall);
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::UpdateObject(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "update";
                apiCall.EntityInfo = (EntityObj)parameters[1];
                apiCall.IsExecuted = false;

                _apiCalls.Add(apiCall);

                var serviceContext = (ServiceContextObj)parameters[0];
                serviceContext.AddCall(apiCall);
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::DeleteObject(Microsoft.Xrm.Sdk.Entity)")
            {
                var apiCall = new XrmApiCall();
                apiCall.Message = "delete";
                apiCall.EntityInfo = (EntityObj)parameters[1];
                apiCall.IsExecuted = false;

                _apiCalls.Add(apiCall);

                var serviceContext = (ServiceContextObj)parameters[0];
                serviceContext.AddCall(apiCall);
            }
            else if (method.FullName == "System.Void Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::ClearChanges()")
            {
                var serviceContext = (ServiceContextObj)parameters[0];
                serviceContext.ClearQueue();
            }
            else if (method.FullName == "Microsoft.Xrm.Sdk.SaveChangesResultCollection Microsoft.Xrm.Sdk.Client.OrganizationServiceContext::SaveChanges()")
            {
                var serviceContext = (ServiceContextObj)parameters[0];
                serviceContext.MarkCallsExecuted();

                _stack.Push($"Dummy return value for '{method.FullName}'");
                Debug.WriteLine($"[↑ {_stack.Count}] Return value from {method.FullName}");
            }
            // Add the entity object to the top of the stack
            else if (method.FullName == "Microsoft.Xrm.Sdk.AttributeCollection Microsoft.Xrm.Sdk.Entity::get_Attributes()")
            {
                _stack.Push(parameters[0]);
                Debug.WriteLine($"[↑ {_stack.Count}] Return value from {method.FullName}");
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
            // The called method is part of the loaded dlls
            // Interpret it to get more call information
            else if (methodDef != null && methodDef.Body != null)
            {
                var vm = new PluginGraphVM(_moduleList);
                var (apicalls, returnValue) = vm.Execute(methodDef, parameters);

                _apiCalls.AddRange(apicalls);
                if (returnValue != null)
                {
                    _stack.Push(returnValue);
                    Debug.WriteLine($"[↑ {_stack.Count}] Return value from {methodDef.FullName}");
                }
            }
            else
            {
                if (method.MethodSig.RetType.FullName != "System.Void")
                {
                    _stack.Push($"Dummy return value for '{method.FullName}'");
                    Debug.WriteLine($"[↑ {_stack.Count}] Return value from {method.FullName}");
                }
            }
        }
    }
}
