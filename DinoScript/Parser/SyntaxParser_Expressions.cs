using DinoScript.Code;
using DinoScript.Syntax;

namespace DinoScript.Parser;

// SyntaxParser_Expressions
public partial class SyntaxParser
{
    // 액션 메서드의 반환값은 bool형으로,
    // 해당 액션에 맞는 구문 노드가 생성되었을 경우 true,
    // 그렇지 않을 경우 구문 오류를 뜻하는 false를 반환합니다.


    #region Expression

    bool GroupExpression()
    {
        var token = Tokenizer.Current();
        
        if (token?.Value == "(")
        {
            var expression = Expression();
            token = Tokenizer.NextWithIgnoreWhiteSpace();
            // TODO: 오류 메시지 처리를 위한 루아 스크립트의 check_match함수와 유사한 함수 구현 필요.
            if (token?.Value == ")")
                return true;
        }

        return false;
    }

    bool PrimaryExpression()
    {
        // PreAccessExpression{ . PostAccessExpression}
        bool AccessExpression()
        {
            bool PreAccessExpression()
            {
                // <Identity>
                var token = Tokenizer.Current();

                switch (token?.Type)
                {
                    case TokenType.Mark:
                    {
                        return GroupExpression();
                    }
                }

                return PostAccessExpression();
            }

            bool PostAccessExpression()
            {
                var token = Tokenizer.Current();

                switch (token?.Type)
                {
                    case TokenType.Identifier:
                    case TokenType.NumberLiteral:
                    {
                        if (token.Type == TokenType.Identifier)
                        {
                            // 심볼 테이블에 존재하는지 체크
                            if (token.Value != null && !SymbolTable.ContainsKey(token.Value))
                                return false;
                        }

                        // 토큰에 해당하는 값을 코드 생성 큐에 추가함
                        CodeGenerator.AccessTokenEnqueue(token);
                        return true;
                    }
                }

                return false;
            }

            if (PreAccessExpression())
            {
                // TODO: { . PostAccessExpression}에 대한 구현 예정
                return true;
            }

            return false;
        }

        // <AccessExpression>
        if (AccessExpression())
        {
            return true;
        }

        return false;
    }


    private readonly IReadOnlyDictionary<string, uint> binaryOperatorPriorityTable = new Dictionary<string, uint>()
    {
        ["*"] = (uint)ExpressionTypes.Multiplicative,
        ["/"] = (uint)ExpressionTypes.Multiplicative,
        ["+"] = (uint)ExpressionTypes.Addictive,
        ["-"] = (uint)ExpressionTypes.Addictive,
    };

    
    /// <summary>
    /// 우선 순위에 의한 하위 표현식 구조를 표현합니다.
    /// </summary>
    /// <param name="binaryOperator"></param>
    /// <param name="priority">연산자의 우선순위입니다. 값이 작을 수록 높은 연산 우선 순위를 나타냅니다.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    bool SubExpression(out Token? binaryOperator, uint? priority = uint.MaxValue)
    {
        // (<PrimaryExpression>|<UnaryExpression>){ BinaryOperator SubExpression}

        // TODO: UnaryExpression에 대한 코드 추가 필요 (Lua스크립트의 lparser 코드 참조)
        bool ok = PrimaryExpression();

        if (ok)
        {
            binaryOperator = GetNextBinaryOperatorToken();

            // 확인된 이항 연산자 우선순위가 현재 표현식 우선순위 보다 높으면 재귀 처리
            // 토큰이 연산자가 아닐경우 GetNextBinaryOperatorToken에 의해 null 임
            while (binaryOperator?.Value != null &&
                   binaryOperatorPriorityTable[binaryOperator.Value] <= priority)
            {
                // 연산자를 코드 생성 스택에 우선 푸시해둠.
                CodeGenerator.OperatorTokenPush(binaryOperator);
                Tokenizer.NextWithIgnoreWhiteSpace();
                if (SubExpression(out var nextOperator, binaryOperatorPriorityTable[binaryOperator.Value]))
                {
                    // SubExpression의 재귀가 끝나면 식을 코드화함
                    CodeGenerator.GenerateExpression();
                    // 처리되지 않은 오퍼레이터를 받아 다시 처리 시작
                    binaryOperator = nextOperator;
                }
                else
                {
                    // 하위 표현식이 오지 않을 경우 문법 오류
                    return false;
                }
            }
            
            // 연산자가 처리되지 않았지만 표현식이 정상적으로 끝날 수 있음
            return true;
        }

        // 표현식이 성립되지 않음
        binaryOperator = null;
        return false;
    }

    Token? GetNextBinaryOperatorToken()
    {
        var token = Tokenizer.NextWithIgnoreWhiteSpace();
        if (token == null)
            return null;

        bool isBinaryOperator = token.Type == TokenType.Operator;
        return isBinaryOperator ? token : null;
    }

    #endregion

    bool Expression()
    {
        // <MultiplicativeExpression>
        if (SubExpression(out _))
        {
            return true;
        }

        return false;
    }
}