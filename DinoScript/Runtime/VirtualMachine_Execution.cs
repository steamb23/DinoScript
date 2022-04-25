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
                    // 아무 것도 실행하지 않음
                    break;
                case Opcode.LoadConstantNumber:
                    Memory.Stack.Push((double)code.Operands[0]);
                    break;
                case Opcode.Pop:
                    break;
                case Opcode.Add:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 + v1);
                    break;
                }
                case Opcode.Subtract:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 - v1);
                    break;
                }
                case Opcode.Multiply:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 * v1);
                    break;
                }
                case Opcode.Divide:
                {
                    var v1 = Memory.Stack.PopDouble();
                    var v2 = Memory.Stack.PopDouble();
                    Memory.Stack.Push(v2 / v1);
                    break;
                }
            }
        }
    }
}