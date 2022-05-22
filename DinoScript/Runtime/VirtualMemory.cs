using System.Collections.Generic;
using System.Linq;
using DinoScript.Utils;

namespace DinoScript.Runtime
{
    public class VirtualMemory
    {
        public const int MinimalFunctionStackSize = 16;
        public const int MinimalOperationStackSize = 16;
        
        private readonly Dictionary<ulong, object> objectTable = new Dictionary<ulong, object>();

        // address 난독화에 사용될 임의의 값
        private readonly ulong randomizeValue = Utils.Random.Shared.NextUInt64();

        // 데이터 추가 및 관리에 사용되는 내부 아이디
        private ulong autoAddress = 0;

        public FunctionStack FunctionStack { get; }
        
        public Stack<DinoValue> OperationStack { get; }

        public VirtualMemory(
            int operationStackSize = MinimalOperationStackSize,
            int functionStackSize = MinimalFunctionStackSize)
        {
            OperationStack = new Stack<DinoValue>(operationStackSize);
            FunctionStack = new FunctionStack(functionStackSize);
        }

        public object this[ulong address] => objectTable[unchecked(address - randomizeValue)];

        public bool TryGet(ulong address, out object? @object) => objectTable.TryGetValue(address, out @object);

        public ulong Add(object item)
        {
            if (!objectTable.TryAdd(autoAddress, item))
            {
                // 추가에 실패했을 경우 objectTable 순회하면서 빈 아이디 공간 찾기
                // 단, 이 경우는 정말 최악의 경우이며 발생 했을 경우 큰 성능 문제가 될 수도 있음
                ulong previousId = 0;
                var orderedObjectTable =
                    from pair in objectTable
                    orderby pair.Key
                    select pair;

                foreach (var pair in orderedObjectTable)
                {
                    var distance = unchecked(pair.Key - previousId);
                    if (distance > 1)
                    {
                        // 빈공간 찾음
                        autoAddress = unchecked(previousId + 1);
                        objectTable.Add(autoAddress, item);
                        break;
                    }

                    previousId = pair.Key;
                }
            }

            return unchecked(autoAddress++ + randomizeValue);
        }

        public bool Remove(ulong address)
        {
            address = unchecked(address - randomizeValue);

            return objectTable.Remove(address);
        }
    }
}