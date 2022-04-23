using DinoScript.Code;

namespace DinoScript.Runtime;

public partial class VirtualMachine
{
    private VirtualMemory memory;

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
                memory.Stack.Push((double)code.Operands[0]);
                break;
            case Opcode.Pop:
                break;
            case Opcode.Add:
            {
                var v1 = memory.Stack.PopDouble();
                var v2 = memory.Stack.PopDouble();
                memory.Stack.Push(v2 + v1);
                break;
            }
            case Opcode.Subtract:
            {
                var v1 = memory.Stack.PopDouble();
                var v2 = memory.Stack.PopDouble();
                memory.Stack.Push(v2 - v1);
                break;
            }
            case Opcode.Multiply:
            {
                var v1 = memory.Stack.PopDouble();
                var v2 = memory.Stack.PopDouble();
                memory.Stack.Push(v2 * v1);
                break;
            }
            case Opcode.Divide:
            {
                var v1 = memory.Stack.PopDouble();
                var v2 = memory.Stack.PopDouble();
                memory.Stack.Push(v2 / v1);
                break;
            }
        }
    }
}