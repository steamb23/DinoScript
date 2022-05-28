namespace DinoScript.Parser
{
    public readonly struct IndentationState
    {
        public IndentationState(int indentCount, int depth)
        {
            IndentCount = indentCount;
            Depth = depth;
        }

        public int IndentCount { get; }

        public int Depth { get; }

        public static IndentationState Empty { get; } = new IndentationState(0, 0);
    }
}