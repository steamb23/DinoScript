using DinoScript.Code;
using DinoScript.Syntax;

namespace DinoScript.Parser;

public partial class SyntaxParser
{
    private Tokenizer Tokenizer { get; }

    // TODO: 심볼테이블 형식 변경 예정
    private Dictionary<string, object> SymbolTable { get; } = new();

    private List<InternalCode> codes = new();

    public CodeGenerator CodeGenerator { get; } = new();
    
    public ParserMode ParserMode { get; }

    public SyntaxParser(TextReader textReader, ParserMode parserMode = ParserMode.Statement)
    {
        Tokenizer = new Tokenizer(textReader);
        ParserMode = parserMode;
    }

    public void Next()
    {
        Root();
    }
    
    void Root()
    {
        switch (ParserMode)
        {
            case ParserMode.Expression:
            {
                Expression();
                break;
            }
            case ParserMode.Statement:
                throw new NotImplementedException();
        }
    }
}