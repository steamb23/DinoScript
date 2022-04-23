using DinoScript.Parser;

namespace DinoScript.Runtime;

public class VirtualMachineOptions
{
    public static VirtualMachineOptions Default { get; } = new VirtualMachineOptions();
    
    public ParserMode ParserMode { get; init; } = ParserMode.Full;

    public int StackSize { get; init; } = VirtualStack.DefaultStackSize;
}