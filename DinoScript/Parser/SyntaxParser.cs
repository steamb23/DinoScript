using DinoScript.Syntax;

namespace DinoScript.Parser;

public partial class SyntaxParser
{
    private Tokenizer Tokenizer { get; }

    private Dictionary<string, ISyntaxNode> SymbolTable { get; } = new();

    private Stack<ISyntaxNode> SyntaxStack { get; } = new();

    private ISyntaxNode? RootTree { get; } = null;

    public SyntaxParser(TextReader textReader)
    {
        Tokenizer = new Tokenizer(textReader);
    }
}