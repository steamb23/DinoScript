using DinoScript.Runtime;

namespace DinoScript.Parser
{
    public struct ExpressionDescription
    {
        public ExpressionKind Kind;
        public DinoValue Value;
        public int ValueCodeIndex;
        public int BranchTrueCodeIndex;
        public int BranchFalseCodeIndex;

        public ExpressionDescription(
            ExpressionKind kind = ExpressionKind.None,
            DinoValue value = new DinoValue(),
            int valueCodeIndex = -1,
            int branchTrueCodeIndex = -1,
            int branchFalseCodeIndex = -1)
        {
            Kind = kind;
            Value = value;
            ValueCodeIndex = valueCodeIndex;
            BranchTrueCodeIndex = branchTrueCodeIndex;
            BranchFalseCodeIndex = branchFalseCodeIndex;
        }

        public static ExpressionDescription Empty { get; } = new ExpressionDescription(ExpressionKind.None);
    }

    public enum ExpressionKind
    {
        None,
        ConstantNumber,
        ConstantInteger,
        ConstantBoolean,
        LocalVariable,
        GlobalVariable,
        Branch,
        FunctionCall,
    }
}