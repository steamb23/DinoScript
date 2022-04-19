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
            case Opcode.NoOperation:
                break;
            case Opcode.LoadConstantNumber:
                memory.Stack.Push((double)code.Operands[0]);
                break;
            case Opcode.Pop:
                break;
            case Opcode.Add:
                break;
            case Opcode.Subtract:
                break;
            case Opcode.Multiply:
                break;
            case Opcode.Divide:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}