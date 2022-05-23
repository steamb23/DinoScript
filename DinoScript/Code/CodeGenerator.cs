using System.Collections.Generic;
using DinoScript.Runtime;

namespace DinoScript.Code
{
    public sealed partial class CodeGenerator
    {
        public List<InternalCode> Codes { get; } = new List<InternalCode>();
    }
}