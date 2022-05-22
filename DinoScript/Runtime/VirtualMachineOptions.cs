using DinoScript.Parser;

namespace DinoScript.Runtime
{
    public class VirtualMachineOptions
    {
        public VirtualMachineOptions(
            ParserMode parserMode = ParserMode.Full,
            int operationStackSize = VirtualMemory.MinimalOperationStackSize,
            int functionStackSize = VirtualMemory.MinimalFunctionStackSize)
        {
            this.ParserMode = parserMode;
            this.OperationStackSize = operationStackSize;
            this.FunctionStackSize = functionStackSize;
        }
        
        public static VirtualMachineOptions Default { get; } = new VirtualMachineOptions();
    
        public ParserMode ParserMode { get; }

        public int OperationStackSize { get; }
        
        public int FunctionStackSize { get; }
    }
}