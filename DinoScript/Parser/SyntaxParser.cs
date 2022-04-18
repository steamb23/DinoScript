using DinoScript.Code;
using DinoScript.Syntax;

namespace DinoScript.Parser;

public partial class SyntaxParser
{
    private Tokenizer Tokenizer { get; }

    // TODO: 심볼테이블 형식 변경 예정
    private Dictionary<string, object> SymbolTable { get; } = new();

    private List<IntermediateCode> codes = new();

    public CodeGenerator CodeGenerator { get; } = new();

    public SyntaxParser(TextReader textReader)
    {
        Tokenizer = new Tokenizer(textReader);
    }
    
    bool Root()
    {
        // <Expression>
        if (Expression())
        {
            return true;
        }

        return false;
    }
}