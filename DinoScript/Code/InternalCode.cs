using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DinoScript.Code;

/// <summary>
/// 다이노스크립트의 중간 코드를 나타냅니다.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct InternalCode
{
    public Opcode Opcode { get; }

    public IReadOnlyList<object> Operands { get; }

    private InternalCode(Opcode opcode, params object[] operand)
    {
        Opcode = opcode;
        Operands = operand;
    }

    public static InternalCode Make(Opcode opcode) => new InternalCode(opcode);

    public static InternalCode Make(Opcode opcode, double value)
    {
        switch (opcode)
        {
            case Opcode.LoadConstant:
                return new InternalCode(opcode, value);
            default:
                throw new ArgumentException(null, nameof(opcode));
        }
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('{');
        builder.Append(Opcode);
        if (Operands.Count > 0)
        {
            builder.Append(", ");
            builder.Append(string.Join(", ", Operands));
        }
        builder.Append('}');
        return builder.ToString();
    }
}