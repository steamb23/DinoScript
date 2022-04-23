namespace DinoScript.Runtime;

public class VirtualStack
{
    public const int DefaultStackSize = 1024 * 1024;

    // address 난독화에 사용될 임의의 값
    private readonly ulong randomizeValue = unchecked((ulong)Random.Shared.NextInt64(long.MinValue, long.MaxValue));

    private readonly byte[] stackArray;
    private readonly Stack<int> stackFrame = new(64);

    private int stackCursor = 0;

    public VirtualStack(int stackSize = DefaultStackSize)
    {
        stackArray = new byte[DefaultStackSize];
    }

    public byte this[ulong address] => stackArray[unchecked(address - randomizeValue)];

    public byte[] CopyToArray()
    {
        var arr = new byte[stackCursor];
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

    private void Push(byte value)
    {
        stackArray[stackCursor] = value;
        stackCursor += 1;
    }

    public void Push(double value)
    {
        const int size = sizeof(double);

        Span<byte> buffer = stackalloc byte[size];
        if (BitConverter.TryWriteBytes(buffer, value))
        {
            Push(buffer);
        }
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

    private void Peek(Span<byte> buffer)
    {
        var length = buffer.Length;
        Span<byte> arraySpan = stackArray.AsSpan(stackCursor - length, length);
        arraySpan.CopyTo(buffer);
    }

    #endregion

    #region Pop

    public void Pop(int length = 1)
    {
        Span<byte> arraySpan = stackArray.AsSpan(stackCursor - length, length);
        arraySpan.Fill(0);
        stackCursor -= length;
    }

    public double PopDouble()
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        Pop(buffer);
        return BitConverter.ToDouble(buffer);
    }

    private void Pop(Span<byte> buffer)
    {
        var length = buffer.Length;
        Span<byte> arraySpan = stackArray.AsSpan(stackCursor - length, length);
        arraySpan.CopyTo(buffer);

        arraySpan.Fill(0);
        stackCursor -= length;
    }

    #endregion
}