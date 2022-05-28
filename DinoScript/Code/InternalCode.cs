using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DinoScript.Syntax;

namespace DinoScript.Code
{
    /// <summary>
    /// 다이노스크립트의 중간 코드를 나타냅니다.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct InternalCode
    {
        public Opcode Opcode { get; }

        public IReadOnlyList<object> Operands { get; }

        public Token? Token { get; }

        private InternalCode(Opcode opcode, Token? token, params object[] operand)
        {
            Opcode = opcode;
            Operands = operand;
            Token = token;
        }

        public static InternalCode Make(Opcode opcode, Token? token) => new InternalCode(opcode, token);

        public static InternalCode Make(Opcode opcode, Token? token, double value)
        {
            switch (opcode)
            {
                case Opcode.LoadConstantNumber:
                    return new InternalCode(opcode, token, value);
                default:
                    throw new ArgumentException(null, nameof(opcode));
            }
        }

        public static InternalCode Make(Opcode opcode, Token? token, long value)
        {
            switch (opcode)
            {
                case Opcode.LoadConstantInteger:
                case Opcode.LoadConstantBoolean:
                    return new InternalCode(opcode, token, value);
                default:
                    throw new ArgumentException(null, nameof(opcode));
            }
        }

        public static InternalCode Make(Opcode opcode, Token? token, int value)
        {
            switch (opcode)
            {
                case Opcode.Branch:
                case Opcode.BranchIfFalse:
                case Opcode.BranchIfTrue:
                case Opcode.StoreToLocal:
                    return new InternalCode(opcode, token, value);
                default:
                    throw new ArgumentException(null, nameof(opcode));
            }
        }

        public static InternalCode Make(Opcode opcode, Token? token, ulong value)
        {
            switch (opcode)
            {
                case Opcode.LoadFromLocal:
                case Opcode.StoreToLocal:
                    return new InternalCode(opcode, token, value);
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
}