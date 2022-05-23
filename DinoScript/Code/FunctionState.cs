using System.Collections.Generic;
using System.ComponentModel;

namespace DinoScript.Code
{
    /// <summary>
    /// 함수 호출 시의 함수의 상태 정보를 나타냅니다.
    /// </summary>
    public class FunctionState
    {
        public FunctionState()
        {
        }

        public FunctionState(FunctionState parent, int stackFrameIndex)
        {
            this.Parent = parent;
            this.StackFrameIndex = stackFrameIndex;
        }

        public Dictionary<string, long> LocalSymbolTable { get; } = new Dictionary<string, long>();

        public FunctionState? Parent { get; }

        public int StackFrameIndex { get; }
    }
}