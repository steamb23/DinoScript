using System.Reflection.Emit;
using DinoScript.Code;

namespace DinoScript.Runtime
{
    public partial class VirtualMachine
    {
        public VirtualMemory Memory { get; }

        /// <summary>
        /// 코드를 실행합니다.
        /// </summary>
        void RunCode(InternalCode code)
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
                    Memory.Stack.Push((double)code.Operands[0]);
                    internalCodeIndex++;
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
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 + v1);
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Subtract:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 - v1);
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Multiply:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 * v1);
                    internalCodeIndex++;
                    break;
                }
                case Opcode.Divide:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 / v1);
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
                    var v1 = Memory.Stack.PopBoolean();
                    if (v1)
                        internalCodeIndex = (int)code.Operands[0];
                    else
                        internalCodeIndex++;
                    break;
                }
                case Opcode.BranchIfFalse:
                {
                    var v1 = Memory.Stack.PopBoolean();
                    if (!v1)
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