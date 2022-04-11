namespace DinoScript.Syntax;

/// <summary>
/// 토큰 데이터를 가지는 단말 노드입니다.
/// </summary>
public class SyntaxTerminalNode : ISyntaxNode
{
    public Token Token { get; }

    public SyntaxTerminalNode(Token token)
    {
        this.Token = token;
    }
    
    public SyntaxKind SyntaxKind => SyntaxKind.Terminal;
}