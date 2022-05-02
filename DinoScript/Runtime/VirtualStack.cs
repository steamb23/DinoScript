using System;
using System.Buffers;
using System.Collections.Generic;

namespace DinoScript.Runtime
{
    public class VirtualStack : IDisposable
    {
        public const int DefaultStackSize = 1024 * 1024;

        private static ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

        private readonly byte[] stackArray;
        private readonly Stack<int> stackFrame = new Stack<int>(64);

        private int stackCursor = 0;

        public VirtualStack(int stackSize = DefaultStackSize)
        {
            stackArray = arrayPool.Rent(DefaultStackSize);
        }

        public byte this[int index] => stackArray[index];

        public byte[] CopyToArray()
        {
            var arr = new byte[DefaultStackSize];
            stackArray.CopyTo(arr, 0);
            return arr;
        }

        #region StackFrame

        /// <summary>
        /// 현재 스택 커서 위치에 스택 프레임을 생성합니다.
        /// </summary>
        public void PushStackFrame()
        {
            stackFrame.Push(stackCursor);
        }

        /// <summary>
        /// 스택 프레임을 제거하고 해당 스택 프레임 위치까지 스택 데이터를 지웁니다. 리턴 값이 있을 경우 그 부분의 데이터는 남깁니다.
        /// </summary>
        public void PopStackFrame(int returnValueLength = 0)
        {
            var stackFrameCursor = stackFrame.Pop();
            var resultCursor = stackFrameCursor + returnValueLength;

            Span<byte> arraySpan = stackArray.AsSpan(resultCursor, stackCursor - resultCursor);
            arraySpan.Fill(0);

            stackCursor = resultCursor;
        }

        #endregion

        #region Push

        public void Push(byte value)
        {
            stackArray[stackCursor] = value;
            stackCursor += 1;
        }

        public void Push(double value)
        {
            const int size = sizeof(double);

            Span<byte> buffer = stackalloc byte[size];
            if (BitConverter.TryWriteBytes(buffer, value))
                Push(buffer);
        }

        public void Push(bool value)
        {
            const int size = sizeof(bool);

            Span<byte> buffer = stackalloc byte[size];
            if (BitConverter.TryWriteBytes(buffer, value))
                Push(buffer);
        }

        private void Push(Span<byte> buffer)
        {
            var length = buffer.Length;
            Span<byte> arraySpan = stackArray.AsSpan(stackCursor, length);
            buffer.CopyTo(arraySpan);
            stackCursor += length;
        }

        #endregion

        #region Peek

        public double PeekDouble()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            Peek(buffer);
            return BitConverter.ToDouble(buffer);
        }

        internal int Peek(Span<byte> buffer)
        {
            var length = buffer.Length;
            var index = stackCursor - length;
            if (index < 0)
            {
                length -= index;
                index = 0;
            }

            Span<byte> arraySpan = stackArray.AsSpan(index, length);
            arraySpan.CopyTo(buffer);

            return length;
        }

        #endregion

        #region Pop

        public int Pop(int length = 1)
        {
            var index = stackCursor - length;
            if (index < 0)
            {
                length -= index;
                index = 0;
            }

            Span<byte> arraySpan = stackArray.AsSpan(index, length);
            arraySpan.Fill(0);
            stackCursor -= length;

            return length;
        }

        public byte PopByte()
        {
            Span<byte> buffer = stackalloc byte[sizeof(byte)];
            Pop(buffer);
            return buffer[0];
        }

        public double PopDouble()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            Pop(buffer);
            return BitConverter.ToDouble(buffer);
        }

        public bool PopBoolean()
        {
            Span<byte> buffer = stackalloc byte[sizeof(bool)];
            Pop(buffer);
            return BitConverter.ToBoolean(buffer);
        }

        internal int Pop(Span<byte> buffer)
        {
            var length = buffer.Length;
            var index = stackCursor - length;
            if (index < 0)
            {
                length -= index;
                index = 0;
            }

            Span<byte> arraySpan = stackArray.AsSpan(index, length);
            arraySpan.CopyTo(buffer);

            arraySpan.Fill(0);
            stackCursor -= length;

            return length;
        }

        #endregion

        private void ReleaseUnmanagedResources()
        {
            arrayPool.Return(stackArray);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~VirtualStack()
        {
            ReleaseUnmanagedResources();
        }
    }
}