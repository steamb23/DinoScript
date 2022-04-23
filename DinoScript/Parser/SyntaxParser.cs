using DinoScript.Code;
using DinoScript.Syntax;

namespace DinoScript.Parser;

public partial class SyntaxParser : IDisposable
{
    private Tokenizer Tokenizer { get; }

    // TODO: 심볼테이블 형식 변경 예정
    private Dictionary<string, object> SymbolTable { get; } = new();

    private List<InternalCode> codes = new();

    public CodeGenerator CodeGenerator { get; } = new();

    public ParserMode ParserMode { get; }

    public SyntaxParser(TextReader textReader, ParserMode parserMode = ParserMode.Full)
    {
        Tokenizer = new Tokenizer(textReader);
        ParserMode = parserMode;
    }

    /// <summary>
    /// 토큰을 읽어 다음 내부 코드를 생성합니다.
    /// </summary>
    /// <returns>토크나이저의 텍스트가 끝에 도달하면 false입니다.</returns>
    public bool Next()
    {
        if (Tokenizer.IsEndOfText)
            return false;
        Root();
        return true;
    }

    void Root()
    {
        switch (ParserMode)
        {
            case ParserMode.ExpressionTest:
            {
                Expression();
                break;
            }
            case ParserMode.Full:
                throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        Tokenizer.Dispose();
    }
}