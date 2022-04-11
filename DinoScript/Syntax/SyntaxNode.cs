using DinoScript.Parser;

namespace DinoScript.Syntax;

public class SyntaxNode : ISyntaxNode
{
    public SyntaxKind SyntaxKind { get; }

    public IReadOnlyList<ISyntaxNode> Children => this.children;

    public SyntaxNode(SyntaxKind syntaxKind, IEnumerable<ISyntaxNode> children)
    {
        SyntaxKind = syntaxKind;
        this.children.AddRange(children);
    }

    private readonly List<ISyntaxNode> children = new();
}