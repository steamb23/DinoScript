namespace DinoScript.Syntax;

public interface ISyntaxNode
{
    SyntaxKind SyntaxKind { get; }

    public static ISyntaxNode Make(SyntaxKind syntaxKind, ISyntaxNode syntaxNodes)
    {
        return new SyntaxNode(syntaxKind, new[] { syntaxNodes });
    }

    public static ISyntaxNode Make(SyntaxKind syntaxKind, IEnumerable<ISyntaxNode> syntaxNodes)
    {
        return new SyntaxNode(syntaxKind, syntaxNodes);
    }

    public static ISyntaxNode Make(Token token)
    {
        return new SyntaxTerminalNode(token);
    }

    public static ISyntaxNode Make(SyntaxKind syntaxKind, Stack<ISyntaxNode> syntaxNodes, int count)
    {
        // 뒤집기용 스택
        var reverseStack = new Stack<ISyntaxNode>(count);
        for (int i = 0; i < count; i++)
        {
            // 여기서 예외가 떴을 경우 프로시저가 잘못 설계됬음을 뜻함.
            reverseStack.Push(syntaxNodes.Pop());
        }

        return Make(syntaxKind, reverseStack);
    }
}