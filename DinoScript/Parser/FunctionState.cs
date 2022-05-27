using System.Collections.Generic;

namespace DinoScript.Parser
{
    /// <summary>
    /// 함수 호출 시의 함수의 상태 정보를 나타냅니다.
    /// </summary>
    public class FunctionState
    {
        public FunctionState()
        {
            GlobalRoot = this;
        }

        public FunctionState(FunctionState parent, int stackFrameIndex)
        {
            this.Parent = parent;
            this.GlobalRoot = parent.GlobalRoot;
            this.StackFrameIndex = stackFrameIndex;
        }

        public Dictionary<string, LocalSymbolDescription> SymbolTable { get; } = new Dictionary<string, LocalSymbolDescription>();

        public FunctionState? Parent { get; }
        
        public FunctionState GlobalRoot { get; }

        public int StackFrameIndex { get; }
    }
}