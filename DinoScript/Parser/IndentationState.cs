namespace DinoScript.Parser
{
    public sealed class IndentationState
    {
        public IndentationState(int indentCount, int depth, IndentationState? parent = null)
        {
            IndentCount = indentCount;
            Depth = depth;
            Parent = parent;
        }

        public int IndentCount { get; }

        public int Depth { get; }

        public IndentationState? Parent { get; }
    }
}