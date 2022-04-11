using DinoScript.Syntax;

namespace DinoScript.Parser;

public partial class SyntaxParser
{
    // 액션 메서드의 반환값은 bool형으로,
    // 해당 액션에 맞는 구문 노드가 생성되었을 경우 true,
    // 그렇지 않을 경우 구문 오류를 뜻하는 false를 반환합니다.


    bool RootAction()
    {
        // <Expression>
        if (ExpressionAction())
        {
            SyntaxStack.Push(
                ISyntaxNode.Make(SyntaxKind.Root, SyntaxStack.Pop()));
        }

        return false;
    }

    bool ExpressionAction()
    {
        // <MultiplicativeExpression>
        if (MultiplicativeExpressionAction())
        {
            SyntaxStack.Push(
                ISyntaxNode.Make(SyntaxKind.Expression, SyntaxStack.Pop()));
            return true;
        }

        return false;
    }

    #region Expressions

    /// <summary>
    /// 기본 표현식에 대한 액션입니다.
    /// </summary>
    bool PrimaryExpressionAction()
    {
        // <AccessExpression>
        if (AccessExpressionAction())
        {
            SyntaxStack.Push(
                ISyntaxNode.Make(SyntaxKind.PrimaryExpression, SyntaxStack.Pop()));
            return true;
        }

        return false;
    }

    /// <summary>
    /// 접근 표현식에 대한 액션입니다.
    /// </summary>
    bool AccessExpressionAction()
    {
        // <Identity>
        var token = Tokenizer.Next();
        if (token is { Type: TokenType.Identifier })
        {
            SyntaxStack.Push(
                ISyntaxNode.Make(SyntaxKind.AccessExpression, ISyntaxNode.Make(token)));
            return true;
        }

        return false;
    }

    /// <summary>
    /// 곱셈 표현식에 대한 액션입니다.
    /// </summary>
    bool MultiplicativeExpressionAction()
    {
        // <PrimaryExpression>
        // <PrimaryExpression> * <MultiplicativeExpression>
        // <PrimaryExpression> / <MultiplicativeExpression>
        if (PrimaryExpressionAction())
        {
            // 오퍼레이터 토큰 검사
            var token = Tokenizer.NextWithIgnoreWhiteSpace();

            if (token == null)
            {
                // TODO: 구문 오류
                return false;
            }

            if (token.Type != TokenType.Operator)
            {
                // 오퍼레이터 토큰이 아니면 PrimaryExpression만 포함
                // <PrimaryExpression>
                SyntaxStack.Push(
                    ISyntaxNode.Make(SyntaxKind.MultiplicativeExpression, SyntaxStack.Pop()));
                return true;
            }

            switch ((token as Token<string>)?.Value)
            {
                // 표적 토큰과 일치하면 노드 구성
                // <PrimaryExpression> * <MultiplicativeExpression>
                // <PrimaryExpression> / <MultiplicativeExpression>
                case "*":
                case "/":
                    SyntaxStack.Push(ISyntaxNode.Make(token));
                    if (MultiplicativeExpressionAction())
                    {
                        SyntaxStack.Push(ISyntaxNode.Make(SyntaxKind.MultiplicativeExpression, SyntaxStack, 3));
                        return true;
                    }

                    // 구문 오류
                    return false;
            }
        }

        // 구문 오류
        return false;
    }

    /// <summary>
    /// 덧셈 표현식에 대한 액션입니다.
    /// </summary>
    bool AddictiveExpressionAction()
    {
        // <MultiplicativeExpression>
        // <MultiplicativeExpression> + <AddictiveExpression>
        // <MultiplicativeExpression> - <AddictiveExpression>
        if (MultiplicativeExpressionAction())
        {
            // 오퍼레이터 토큰 검사
            var token = Tokenizer.NextWithIgnoreWhiteSpace();

            if (token == null)
            {
                // 구문 오류
                return false;
            }

            if (token.Type != TokenType.Operator)
            {
                // 오퍼레이터 토큰이 아니면 PrimaryExpression만 포함
                // <PrimaryExpression>
                SyntaxStack.Push(ISyntaxNode.Make(SyntaxKind.MultiplicativeExpression, new[]
                {
                    SyntaxStack.Pop()
                }));
                return true;
            }

            switch ((token as Token<string>)?.Value)
            {
                // 표적 토큰과 일치하면 노드 구성
                // <MultiplicativeExpression> + <AddictiveExpression>
                // <MultiplicativeExpression> - <AddictiveExpression>
                case "+":
                case "-":
                    SyntaxStack.Push(ISyntaxNode.Make(token));
                    if (MultiplicativeExpressionAction())
                    {
                        SyntaxStack.Push(ISyntaxNode.Make(SyntaxKind.MultiplicativeExpression, SyntaxStack, 3));
                        return true;
                    }

                    // 구문 오류
                    return false;
            }
        }

        // 구문 오류
        return false;
    }

    #endregion
}