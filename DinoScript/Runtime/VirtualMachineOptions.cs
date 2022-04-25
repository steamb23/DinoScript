using DinoScript.Parser;

namespace DinoScript.Runtime
{
    public class VirtualMachineOptions
    {
        public VirtualMachineOptions(
            ParserMode parserMode = ParserMode.Full,
            int stackSize = VirtualStack.DefaultStackSize)
        {
            this.ParserMode = parserMode;
            this.StackSize = stackSize;
        }
        
        public static VirtualMachineOptions Default { get; } = new VirtualMachineOptions();
    
        public ParserMode ParserMode { get; }

        public int StackSize { get; }
    }
}