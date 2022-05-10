using System.Collections.Generic;

namespace DinoScript.Runtime
{
    public class VirtualStack
    {
        public const int MinimalStackSize = 64;


        private readonly List<DinoValue> stackArray;
        private readonly Stack<int> stackFrame;

        public VirtualStack(int stackSize = MinimalStackSize)
        {
            stackArray = new List<DinoValue>(stackSize);
            stackFrame = new Stack<int>(MinimalStackSize / 4);
        }

        public DinoValue this[int index]
        {
            get => stackArray[index];
            set => stackArray[index] = value;
        }

        public DinoValue[] CopyToArray()
        {
            var arr = new DinoValue[stackArray.Count];
            stackArray.CopyTo(arr, 0);
            return arr;
        }

        #region StackFrame

        /// <summary>
        /// 현재 스택 커서 위치에 스택 프레임을 생성합니다.
        /// </summary>
        public void CreateStackFrame()
        {
            stackFrame.Push(stackArray.Count);
        }

        /// <summary>
        /// 스택 프레임을 제거하고 해당 스택 프레임 위치까지 스택 데이터를 지웁니다. 리턴 값이 있을 경우 그 부분의 데이터는 남깁니다.
        /// </summary>
        public void RemoveStackFrame(bool hasReturnValue)
        {
            var stackFrameCursor = stackFrame.Pop();
            var resultCursor = stackFrameCursor + (hasReturnValue ? 1 : 0);

            stackArray.RemoveRange(resultCursor, stackArray.Count - resultCursor);
        }

        public int CurrentStackFrameIndex => stackFrame.TryPeek(out var result) ? result : 0;

        #endregion

        #region Pop

        public DinoValue Pop()
        {
            var value = Peek();
            stackArray.RemoveAt(stackArray.Count - 1);
            return value;
        }

        #endregion

        #region Peek

        public DinoValue Peek() => stackArray[^1];

        #endregion

        #region Push

        public void Push(DinoValue value) => stackArray.Add(value);

        #endregion
    }
}