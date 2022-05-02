using System;
using System.Buffers;
using System.Collections.Generic;

namespace DinoScript.Runtime
{
    public class VirtualStack
    {
        public const int MinimalStackSize = 64;


        private readonly List<long> stackArray;
        private readonly Stack<int> stackFrame;

        public VirtualStack(int stackSize = MinimalStackSize)
        {
            stackArray = new List<long>(stackSize);
            stackFrame = new Stack<int>(MinimalStackSize / 4);
        }

        public long this[int index] => stackArray[index];

        public long[] CopyToArray()
        {
            var arr = new long[stackArray.Count];
            stackArray.CopyTo(arr, 0);
            return arr;
        }

        #region StackFrame

        /// <summary>
        /// 현재 스택 커서 위치에 스택 프레임을 생성합니다.
        /// </summary>
        public void PushStackFrame()
        {
            stackFrame.Push(stackArray.Count);
        }

        /// <summary>
        /// 스택 프레임을 제거하고 해당 스택 프레임 위치까지 스택 데이터를 지웁니다. 리턴 값이 있을 경우 그 부분의 데이터는 남깁니다.
        /// </summary>
        public void PopStackFrame(int returnValueLength = 0)
        {
            var stackFrameCursor = stackFrame.Pop();
            var resultCursor = stackFrameCursor + returnValueLength;

            stackArray.RemoveRange(resultCursor, stackArray.Count - resultCursor);
        }

        #endregion

        #region Pop

        public long Pop()
        {
            var value = Peek();
            stackArray.RemoveAt(stackArray.Count - 1);
            return value;
        }

        public double PopDouble() => BitConverter.Int64BitsToDouble(Pop());

        #endregion

        #region Peek

        public long Peek() => stackArray[^1];

        public double PeekDouble() => BitConverter.Int64BitsToDouble(Peek());

        #endregion

        #region Push

        public void Push(long value) => stackArray.Add(value);

        public void Push(double value) => Push(BitConverter.DoubleToInt64Bits(value));

        #endregion
    }
}