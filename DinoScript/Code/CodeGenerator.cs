using System.Collections.Generic;
using DinoScript.Runtime;
using DinoScript.Syntax;

namespace DinoScript.Code
{
    public sealed partial class CodeGenerator
    {
        public const int NoJump = -1;

        public List<InternalCode> Codes { get; } = new List<InternalCode>();

        public void Duplicate(in Token token)
        {
            Codes.Add(InternalCode.Make(Opcode.Duplicate, token));
        }
    }
}