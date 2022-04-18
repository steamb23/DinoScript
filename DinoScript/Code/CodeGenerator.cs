using DinoScript.Syntax;

namespace DinoScript.Code;

public class CodeGenerator
{
    private List<IntermediateCode> codes = new();

    private Stack<IntermediateCode> expressionStack = new();
    private Queue<IntermediateCode> expressionQueue = new();

    public IReadOnlyList<IntermediateCode> Codes => codes;

    /// <summary>
    /// 토큰의 값을 코드 큐에 넣습니다.
    /// </summary>
    /// <param name="token"></param>
    public void AccessTokenEnqueue(Token token)
    {
        IntermediateCode code = token.Type switch
        {
            TokenType.NumberLiteral => IntermediateCode.Make(Opcode.Push, double.Parse(token.Value!)),
            _ => IntermediateCode.Make(Opcode.NoOperation)
        };

        expressionQueue.Enqueue(code);
    }

    /// <summary>
    /// 토큰을 연산자로 재분류하고 코드 스택에 넣습니다.
    /// </summary>
    /// <param name="token"></param>
    public void OperatorTokenPush(Token token)
    {
        var opcode = token.Value switch
        {
            "+" => Opcode.Add,
            "-" => Opcode.Subtract,
            "*" => Opcode.Multiply,
            "/" => Opcode.Divide,
            _ => Opcode.NoOperation
        };

        expressionStack.Push(IntermediateCode.Make(opcode));
    }

    /// <summary>
    /// 표현식 코드의 축적을 마치고 코드 리스트에 출력합니다.
    /// </summary>
    public void GenerateExpression()
    {
        codes.AddRange(expressionQueue);
        expressionQueue.Clear();
        codes.AddRange(expressionStack);
        expressionStack.Clear();
    }
}