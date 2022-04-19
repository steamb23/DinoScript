namespace DinoScript.Runtime;

public class VirtualStack
{
    public const int DefaultStackSize = 1024 * 1024;

    private readonly byte[] stackArray;

    private int stackCursor = 0;

    public VirtualStack(int stackSize = DefaultStackSize)
    {
        stackArray = new byte[DefaultStackSize];
    }

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

    public void Peek(Span<byte> buffer)
    {
        var length = buffer.Length;
        Span<byte> arraySpan = stackArray.AsSpan(stackCursor - length, length);
        arraySpan.CopyTo(buffer);
    }

    public void Pop(int length = 1)
    {
        Span<byte> arraySpan = stackArray.AsSpan(stackCursor - length, length);
        arraySpan.Fill(0);
        stackCursor -= length;
    }

    public void Pop(Span<byte> buffer)
    {
        var length = buffer.Length;
        Span<byte> arraySpan = stackArray.AsSpan(stackCursor - length, length);
        arraySpan.CopyTo(buffer);
        
        arraySpan.Fill(0);
        stackCursor -= length;
    }
}