using DinoScript.Runtime;

namespace DinoScript
{
    /// <summary>
    /// 스택의 상위 혹은 특정 버퍼 데이터에 대한 변환 기능을 제공하는 뷰를 나타냅니다.
    /// </summary>
    public class ResultView
    {
        private readonly DinoValue value;

        private VirtualMemory? memory = null;

        internal ResultView(VirtualMemory memory)
        {
            value = memory.OperationStack.Peek();
            this.memory = memory;
        }

        internal ResultView(DinoValue value, VirtualMemory? memory = null)
        {
            this.value = value;
            this.memory = memory;
        }

        public DinoValue Value => value;

        // TODO: 추후 스크립트 내에 객체 혹은 테이블 구현이 추가될 경우 적절한 변환 필요
        public object? Object
        {
            get
            {
                if (memory == null)
                    return null;

                memory.TryGet(unchecked((ulong)value.Int64), out var resultObject);

                return resultObject;
            }
        }

        public string? String => Object as string;

        public double? Double => value.Double;
    }
}