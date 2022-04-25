using System;
using DinoScript.Runtime;

namespace DinoScript
{
    /// <summary>
    /// 스택의 상위 혹은 특정 버퍼 데이터에 대한 변환 기능을 제공하는 뷰를 나타냅니다.
    /// </summary>
    public class ResultView
    {
        const int FullSize = sizeof(long);

        private byte[] buffer = new byte[FullSize];
        private int bufferSize = 0;

        private VirtualMemory? memory = null;

        public ResultView(VirtualMemory memory)
        {
            bufferSize = memory.Stack.Peek(buffer);
            this.memory = memory;
        }

        public ResultView(Span<byte> buffer, VirtualMemory? memory = null)
        {
            buffer.CopyTo(this.buffer);
            this.memory = memory;
        }

        // TODO: 추후 스크립트 내에 객체 혹은 테이블 구현이 추가될 경우 적절한 변환 필요
        public object? Object
        {
            get
            {
                if (memory == null || bufferSize != FullSize)
                    return null;

                var int64Result = BitConverter.ToUInt64(buffer);
                memory.TryGet(int64Result, out var resultObject);

                return resultObject;
            }
        }

        public string? String => Object as string;

        public double? Double => bufferSize == FullSize ? BitConverter.ToDouble(buffer) : (double?)null;
    }
}