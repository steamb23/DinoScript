using System.Collections.Generic;

namespace DinoScript.Parser
{
    /// <summary>
    /// 함수 호출 시의 함수의 상태 정보를 나타냅니다.
    /// </summary>
    public readonly struct FunctionState
    {
        public FunctionState(int stackFrameIndex)
        {
            StackFrameIndex = stackFrameIndex;
            SymbolTable = new Dictionary<string, LocalSymbolDescription>();
        }
        
        public FunctionState(int stackFrameIndex, Dictionary<string, LocalSymbolDescription> symbolTable)
        {
            StackFrameIndex = stackFrameIndex;
            SymbolTable = symbolTable;
        }

        public Dictionary<string, LocalSymbolDescription> SymbolTable { get; }

        public int StackFrameIndex { get; }
    }
}