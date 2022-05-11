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
                case Opcode.NoOperation:
                {
                    // 아무 것도 실행하지 않음
                    internalCodeIndex++;
                    break;
                }

                #region 적재 및 저장

                case Opcode.LoadConstantNumber:
                {
                    Memory.Stack.Push(DinoValue.Number((double)code.Operands[0]));
                    internalCodeIndex++;
                    break;
                }

                case Opcode.LoadFromLocal:
                {
                    var localIndex = (int)code.Operands[0];
                    var stackIndex = Memory.Stack.CurrentStackFrameIndex;
                    Memory.Stack.Push(Memory.Stack[stackIndex]);
                    break;
                }

                case Opcode.StoreToLocal:
                {
                    var localIndex = (int)code.Operands[0];
                    var stackIndex = Memory.Stack.CurrentStackFrameIndex;
                    Memory.Stack[stackIndex] = Memory.Stack.Pop();
                    break;
                }

                #endregion

                #region 실수 사칙연산

                case Opcode.Pop:
                {
                    Memory.Stack.Pop();
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Add:
                {
                    var v1 = Memory.Stack.Pop();
                    var v2 = Memory.Stack.Pop();

                    if (v1.Type == DinoType.Number)
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 + (double)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 + (double)v1);
                        }
                    }
                    else // v1.Type == DinoType.Integer or etc
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 + (long)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 + (long)v1);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Subtract:
                {
                    var v1 = Memory.Stack.Pop();
                    var v2 = Memory.Stack.Pop();

                    if (v1.Type == DinoType.Number)
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 - (double)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 - (double)v1);
                        }
                    }
                    else // v1.Type == DinoType.Integer or etc
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 - (long)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 - (long)v1);
                        }
                    }

                    internalCodeIndex++;
                    break;
                }
                case Opcode.Multiply:
                {
                    var v1 = Memory.Stack.Pop();
                    var v2 = Memory.Stack.Pop();

                    if (v1.Type == DinoType.Number)
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 * (double)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 * (double)v1);
                        }
                    }
                    else // v1.Type == DinoType.Integer or etc
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 * (long)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 * (long)v1);
                        }
                    }
                    
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Divide:
                {
                    var v1 = Memory.Stack.Pop();
                    var v2 = Memory.Stack.Pop();

                    if (v1.Type == DinoType.Number)
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 / (double)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 / (double)v1);
                        }
                    }
                    else // v1.Type == DinoType.Integer or etc
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 / (long)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 / (long)v1);
                        }
                    }
                    
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Modulo:
                {
                    var v1 = Memory.Stack.Pop();
                    var v2 = Memory.Stack.Pop();

                    if (v1.Type == DinoType.Number)
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 % (double)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 % (double)v1);
                        }
                    }
                    else // v1.Type == DinoType.Integer or etc
                    {
                        if (v2.Type == DinoType.Number)
                        {
                            Memory.Stack.Push((double)v2 % (long)v1);
                        }
                        else
                        {
                            Memory.Stack.Push((long)v2 % (long)v1);
                        }
                    }
                    
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Negative:
                {
                    var v1 = Memory.Stack.Pop();
                    if (v1.Type== DinoType.Number)
                        Memory.Stack.Push(-(double)v1);
                    else
                        Memory.Stack.Push(-(long)v1);
                    internalCodeIndex++;
                    break;
                }

                #endregion

                #region 분기

                case Opcode.Branch:
                {
                    internalCodeIndex = (int)code.Operands[0];
                    break;
                }
                case Opcode.BranchIfTrue:
                {
                    var v1 = Memory.Stack.Pop();
                    if (v1.Int64 != 0)
                        internalCodeIndex = (int)code.Operands[0];
                    else
                        internalCodeIndex++;
                    break;
                }
                case Opcode.BranchIfFalse:
                {
                    var v1 = Memory.Stack.Pop();
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