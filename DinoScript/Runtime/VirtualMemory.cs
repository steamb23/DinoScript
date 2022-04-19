namespace DinoScript.Runtime;

public class VirtualMemory
{
    private readonly Dictionary<ulong, object> objectTable = new();

    // id 난독화에 사용될 임의의 값
    private readonly ulong randomizeValue = unchecked((ulong)Random.Shared.NextInt64());

    // 데이터 추가 및 관리에 사용되는 내부 아이디
    private ulong autoId = 0;

    public VirtualStack Stack { get; }

    public VirtualMemory(int stackSize = VirtualStack.DefaultStackSize)
    {
        Stack = new VirtualStack(stackSize);
    }

    public object this[ulong id] => objectTable[id];

    public ulong Add(object item)
    {
        if (!objectTable.TryAdd(autoId, item))
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
                    autoId = unchecked(previousId + 1);
                    objectTable.Add(autoId, item);
                    break;
                }

                previousId = pair.Key;
            }
        }

        return unchecked(autoId++ + randomizeValue);
    }

    public bool Remove(ulong id)
    {
        id = unchecked(id - randomizeValue);

        return objectTable.Remove(id);
    }
}