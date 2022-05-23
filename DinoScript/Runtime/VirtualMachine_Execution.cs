using System;
using DinoScript.Code;

namespace DinoScript.Runtime
{
    public partial class VirtualMachine
    {
        public VirtualMemory Memory { get; }

        /// <summary>
        /// 코드를 실행합니다.
        /// </summary>
        private void RunCode(InternalCode code)
        {
            switch (code.Opcode)
            {
                default:
                {
                    throw new NotImplementedException($"Opcode {code.Opcode}의 구현이 되지 않았습니다.");
                }
                case Opcode.NoOperation:
                {
                    // 아무 것도 실행하지 않음
                    internalCodeIndex++;
                    break;
                }

                case Opcode.Pop:
                {
                    Memory.OperationStack.Pop();
                    internalCodeIndex++;
                    break;
                }

                #region 적재 및 저장

                case Opcode.LoadConstantNumber:
                {
                    Memory.OperationStack.Push(DinoValue.Number((double)code.Operands[0]));
                    internalCodeIndex++;
                    break;
                }

                case Opcode.LoadConstantInteger:
                {
                    Memory.OperationStack.Push(DinoValue.Integer((long)code.Operands[0]));
                    internalCodeIndex++;
                    break;
                }

                case Opcode.LoadConstantBoolean:
                {
                    Memory.OperationStack.Push(DinoValue.Boolean((long)code.Operands[0]));
                    internalCodeIndex++;
                    break;
                }

                case Opcode.LoadFromLocal:
                {
                    var localIndex = (int)code.Operands[0];
                    var functionStackIndex = Memory.FunctionStack.CurrentStackFrameIndex + localIndex;
                    Memory.OperationStack.Push(Memory.FunctionStack[functionStackIndex]);
                    break;
                }

                case Opcode.StoreToLocal:
                {
                    // var localIndex = (int)code.Operands[0];
                    // var stackIndex = Memory.OperationStack.CurrentStackFrameIndex;
                    // Memory.OperationStack[stackIndex] = Memory.OperationStack.Pop();
                    var localIndex = (int)code.Operands[0];
                    var functionStackIndex = Memory.FunctionStack.CurrentStackFrameIndex + localIndex;
                    Memory.FunctionStack[functionStackIndex] = Memory.OperationStack.Pop();
                    break;
                }

                case Opcode.StoreToNewLocal:
                {
                    Memory.FunctionStack.Push(Memory.OperationStack.Pop());
                    break;
                }

                #endregion

                #region 수식 연산

                case Opcode.Add:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 + (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 + (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 + (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 + (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Subtract:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 - (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 - (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 - (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 - (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Multiply:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 * (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 * (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 * (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 * (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Divide:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 / (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 / (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 / (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((double)(long)v1 / (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Modulo:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 % (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 % (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 % (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 % (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Negative:
                {
                    var v1 = Memory.OperationStack.Pop();
                    if (v1.Type == DinoType.Number)
                        Memory.OperationStack.Push(-(double)v1);
                    else
                        Memory.OperationStack.Push(-(long)v1);
                    internalCodeIndex++;
                    break;
                }

                #endregion

                #region 비교 연산

                // ReSharper disable CompareOfFloatsByEqualityOperator
                case Opcode.Equal:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 == (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 == (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 == (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 == (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.NotEqual:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 != (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 != (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 != (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 != (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.GreaterThanOrEqual:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 >= (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 >= (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 >= (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 >= (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.LessThanOrEqual:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 <= (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 <= (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 <= (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 <= (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.GreaterThan:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 > (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 > (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 > (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 > (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.LessThan:
                {
                    var v2 = Memory.OperationStack.Pop();
                    var v1 = Memory.OperationStack.Pop();

                    if (v2.Type == DinoType.Number)
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 < (double)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 < (double)v2);
                        }
                    }
                    else // v2.Type == DinoType.Integer or etc
                    {
                        if (v1.Type == DinoType.Number)
                        {
                            Memory.OperationStack.Push((double)v1 < (long)v2);
                        }
                        else
                        {
                            Memory.OperationStack.Push((long)v1 < (long)v2);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator

                #endregion

                #region 분기

                case Opcode.Branch:
                {
                    internalCodeIndex = (int)code.Operands[0];
                    break;
                }
                case Opcode.BranchIfTrue:
                {
                    var v1 = Memory.OperationStack.Pop();
                    if (v1.Int64 != 0)
                        internalCodeIndex = (int)code.Operands[0];
                    else
                        internalCodeIndex++;
                    break;
                }
                case Opcode.BranchIfFalse:
                {
                    var v1 = Memory.OperationStack.Pop();
                    if (v1.Int64 == 0)
                        internalCodeIndex = (int)code.Operands[0];
                    else
                        internalCodeIndex++;
                    break;
                }

                #endregion
            }
        }
    }
}