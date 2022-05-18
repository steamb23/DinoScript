using DinoScript.Runtime;

namespace DinoScript.Code.Generator
{
    public struct ExpressionDescription
    {
        public ExpressionKind Kind;
        public DinoValue Value;
        public int ValueCodeIndex;
        public int BranchInfo;
        public int BranchTrueCodeIndex;
        public int BranchFalseCodeIndex;

        public ExpressionDescription(
            ExpressionKind kind = ExpressionKind.None,
            DinoValue value = new DinoValue(),
            int valueCodeIndex = -1,
            int branchInfo = -1,
            int branchTrueCodeIndex = -1,
            int branchFalseCodeIndex = -1)
        {
            Kind = kind;
            Value = value;
            ValueCodeIndex = valueCodeIndex;
            BranchInfo = branchInfo;
            BranchTrueCodeIndex = branchTrueCodeIndex;
            BranchFalseCodeIndex = branchFalseCodeIndex;

        }

        public static ExpressionDescription Empty => new ExpressionDescription(ExpressionKind.None);
    }

    public enum ExpressionKind
    {
        None,
        Constant,
        LocalVariable,
        GlobalVariable,
        Branch,
        FunctionCall,
    }
}