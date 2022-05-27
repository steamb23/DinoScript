namespace DinoScript.Parser
{
    public readonly struct LocalSymbolDescription
    {
        public LocalSymbolDescription(int localIndex, int localBlockDepth)
        {
            LocalIndex = localIndex;
            LocalBlockDepth = localBlockDepth;
        }

        public int LocalIndex { get; }
        public int LocalBlockDepth { get; }
    }
}