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
            DinoValue v2;
            DinoValue v1;
            switch (code.Opcode)
            {
                default:
                    throw new NotImplementedException($"Opcode {code.Opcode}의 구현이 되지 않았습니다.");
                case Opcode.NoOperation:
                    // 아무 것도 실행하지 않음
                    internalCodeIndex++;
                    break;

                case Opcode.Pop:
                    Memory.OperationStack.Pop();
                    internalCodeIndex++;
                    break;

                #region 적재 및 저장

                case Opcode.LoadConstantNumber:
                    Memory.OperationStack.Push(DinoValue.Number((double)code.Operands[0]));
                    internalCodeIndex++;
                    break;

                case Opcode.LoadConstantInteger:
                    Memory.OperationStack.Push(DinoValue.Integer((long)code.Operands[0]));
                    internalCodeIndex++;
                    break;

                case Opcode.LoadConstantBoolean:
                    Memory.OperationStack.Push(DinoValue.Boolean((long)code.Operands[0]));
                    internalCodeIndex++;
                    break;

                case Opcode.LoadFromLocal:
                    Memory.OperationStack.Push(
                        Memory.FunctionStack[
                            Memory.FunctionStack.CurrentStackFrameIndex + (int)code.Operands[0]]);

                    internalCodeIndex++;
                    break;

                case Opcode.StoreToLocal:
                    Memory.FunctionStack[
                        Memory.FunctionStack.CurrentStackFrameIndex + (int)code.Operands[0]
                    ] = Memory.OperationStack.Pop();
                    internalCodeIndex++;
                    break;

                case Opcode.StoreToNewLocal:
                    Memory.FunctionStack.Push(Memory.OperationStack.Pop());
                    internalCodeIndex++;
                    break;

                #endregion

                #region 수식 연산

                case Opcode.Add:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.Subtract:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.Multiply:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.Divide:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.Modulo:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.Negative:
                    v1 = Memory.OperationStack.Pop();
                    if (v1.Type == DinoType.Number)
                        Memory.OperationStack.Push(-(double)v1);
                    else
                        Memory.OperationStack.Push(-(long)v1);
                    internalCodeIndex++;
                    break;

                #endregion

                #region 비교 연산

                // ReSharper disable CompareOfFloatsByEqualityOperator
                case Opcode.Equal:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.NotEqual:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.GreaterThanOrEqual:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.LessThanOrEqual:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.GreaterThan:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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

                case Opcode.LessThan:
                    v2 = Memory.OperationStack.Pop();
                    v1 = Memory.OperationStack.Pop();

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
                // ReSharper restore CompareOfFloatsByEqualityOperator

                #endregion

                #region 분기

                case Opcode.Branch:
                    internalCodeIndex = (int)code.Operands[0];
                    break;

                case Opcode.BranchIfTrue:
                    v1 = Memory.OperationStack.Pop();
                    if (v1.Int64 != 0)
                        internalCodeIndex = (int)code.Operands[0];
                    else
                        internalCodeIndex++;
                    break;

                case Opcode.BranchIfFalse:
                    v1 = Memory.OperationStack.Pop();
                    if (v1.Int64 == 0)
                        internalCodeIndex = (int)code.Operands[0];
                    else
                        internalCodeIndex++;
                    break;

                #endregion
            }
        }
    }
}