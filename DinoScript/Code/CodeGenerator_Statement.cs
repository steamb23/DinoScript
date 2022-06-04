using System;
using System.Reflection.Emit;
using DinoScript.Parser;
using DinoScript.Runtime;
using DinoScript.Syntax;

namespace DinoScript.Code
{
    public partial class CodeGenerator
    {
        /// <summary>
        /// if 조건 검사가 실패했을 경우 건너뛰는 명령을 생성합니다.
        /// </summary>
        public int IfNotBranch(Token? token)
        {
            var codePos = Codes.Count;
            Codes.Add(InternalCode.Make(Opcode.BranchIfFalse, token, NoJump));
            return codePos;
        }

        /// <summary>
        /// if 문이 끝나 탈출할경우 명령을 생성합니다.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="escapeChain"></param>
        /// <returns></returns>
        public int IfEscape(Token? token, int escapeChain)
        {
            var codePos = Codes.Count;
            Codes.Add(InternalCode.Make(Opcode.Branch, token, escapeChain));
            return codePos;
        }
    }
}