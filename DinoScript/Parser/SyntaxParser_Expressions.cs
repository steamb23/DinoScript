using System;
using System.Collections.Generic;
using DinoScript.Code.Generator;
using DinoScript.Runtime;
using DinoScript.Syntax;

namespace DinoScript.Parser
{
    // SyntaxParser_Expressions
    public partial class SyntaxParser
    {
        // 액션 메서드의 반환값은 bool형으로,
        // 해당 액션에 맞는 구문 노드가 생성되었을 경우 true,
        // 그렇지 않을 경우 구문 오류를 뜻하는 false를 반환합니다.

        private void Expression(ref ExpressionDescription expressionDescription)
        {
            SubExpression(out _, ref expressionDescription, uint.MaxValue);
            CodeGenerator.ExpressionEnd(ref expressionDescription);
            // 표현식 코드 생성이 안됬을 경우 강제 생성 (PrimaryExpression만 있고 연산자가 없는 경우)
            // CodeGeneratorLegacy.GenerateExpression(BinaryOperator.NoBinaryOperator);
        }

        private void GroupExpression(ref ExpressionDescription expressionDescription)
        {
            var token = Tokenizer.Current();

            if (token?.Value == "(")
            {
                // 토큰 넘어가기
                Tokenizer.NextWithIgnoreWhiteSpace();
                Expression(ref expressionDescription);
                token = Tokenizer.Current();
                // TODO: 오류 메시지 처리를 위한 루아 스크립트의 check_match함수와 유사한 함수 구현 필요.
                if (token?.Value == ")")
                {
                    return;
                }

                throw new SyntaxErrorException(Tokenizer.Current(), "Grouped expressions must end with a ')'.");
            }

            // 내부 오류
            throw new SyntaxErrorException(Tokenizer.Current(), $"{token} is not group expression.");
        }

        #region PrimaryExpression

        private void PrimaryExpression(ref ExpressionDescription expressionDescription)
        {
            PreAccessExpression(ref expressionDescription);
        }

        void PreAccessExpression(ref ExpressionDescription expressionDescription)
        {
            var token = Tokenizer.Current();

            // <Identity>
            switch (token?.Type)
            {
                case TokenType.Mark:
                {
                    GroupExpression(ref expressionDescription);
                    return;
                }
            }

            PostAccessExpression(ref expressionDescription);
        }

        void PostAccessExpression(ref ExpressionDescription expressionDescription)
        {
            var token = Tokenizer.Current();

            switch (token?.Type)
            {
                case TokenType.Identifier:
                {
                    // 심볼 테이블에 존재하는지 체크
                    if (token.Value != null && !CurrentFunctionState.LocalSymbolTable.ContainsKey(token.Value))
                        throw new SyntaxErrorException(token, $"'{token.Value}' symbol does not exist.");

                    CodeGenerator.ExpressionInitialize(
                        out expressionDescription,
                        ExpressionKind.Variable,
                        0,
                        token);
                    return;
                }
                case TokenType.NumberLiteral:
                {
                    if (long.TryParse(token.Value!, out var longValue))
                    {
                        CodeGenerator.ExpressionInitialize(
                            out expressionDescription,
                            ExpressionKind.Constant,
                            longValue,
                            token);
                    }
                    else
                    {
                        CodeGenerator.ExpressionInitialize(
                            out expressionDescription,
                            ExpressionKind.Constant,
                            double.Parse(token.Value!),
                            token);
                    }

                    return;
                }
                case TokenType.BooleanLiteral:
                {
                    CodeGenerator.ExpressionInitialize(
                        out expressionDescription,
                        ExpressionKind.Constant,
                        bool.Parse(token.Value!),
                        token);
                    return;
                }
            }

            throw new SyntaxErrorException(token);
        }

        #endregion
        
        private readonly IReadOnlyDictionary<BinaryOperator, uint> binaryOperatorPriorityTable =
            new Dictionary<BinaryOperator, uint>()
            {
                [BinaryOperator.Multiply] = (uint)ExpressionTypes.Multiplicative,
                [BinaryOperator.Divide] = (uint)ExpressionTypes.Multiplicative,
                [BinaryOperator.Modulo] = (uint)ExpressionTypes.Multiplicative,

                [BinaryOperator.Add] = (uint)ExpressionTypes.Addictive,
                [BinaryOperator.Subtract] = (uint)ExpressionTypes.Addictive,

                [BinaryOperator.Equal] = (uint)ExpressionTypes.Equality,
                [BinaryOperator.NotEqual] = (uint)ExpressionTypes.Equality,

                [BinaryOperator.GreaterThanOrEqual] = (uint)ExpressionTypes.Comparison,
                [BinaryOperator.LessThanOrEqual] = (uint)ExpressionTypes.Comparison,
                [BinaryOperator.GreaterThan] = (uint)ExpressionTypes.Comparison,
                [BinaryOperator.LessThan] = (uint)ExpressionTypes.Comparison,

                [BinaryOperator.And] = (uint)ExpressionTypes.LogicalAnd,
                [BinaryOperator.Or] = (uint)ExpressionTypes.LogicalOr,
            };

        /// <summary>
        /// 우선 순위에 의한 하위 표현식 구조를 표현합니다.
        /// </summary>
        /// <param name="binaryOperator">함수 내에서 처리되지 않은 연산자를 반환합니다.</param>
        /// <param name="exprDesc">식의 설명자입니다.</param>
        /// <param name="priority">연산자의 우선순위입니다. 값이 작을 수록 높은 연산 우선 순위를 나타냅니다.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private void SubExpression(
            out BinaryOperator binaryOperator,
            ref ExpressionDescription exprDesc,
            uint priority)
        {
            // (<PrimaryExpression>|<UnaryExpression>){ BinaryOperator SubExpression}

            var token = Tokenizer.Current();
            var unaryOperator = CheckUnaryOperator(token);
            if (unaryOperator != UnaryOperator.NoUnaryOperator)
            {
                // UnaryExpression
                Tokenizer.NextWithIgnoreWhiteSpace();
                SubExpression(out _, ref exprDesc, (uint)ExpressionTypes.Unary);
                CodeGenerator.ExpressionPreProcessing(unaryOperator, ref exprDesc, token!);
            }
            else
            {
                PrimaryExpression(ref exprDesc);
            }

            token = Tokenizer.NextWithIgnoreWhiteSpace();
            binaryOperator = CheckBinaryOperator(token);
            // 확인된 이항 연산자 우선순위가 현재 표현식 우선순위 보다 높으면 재귀 처리
            // 토큰이 연산자가 아닐경우 GetNextBinaryOperatorToken에 의해 null 임
            uint operatorPriority;
            while (binaryOperator != BinaryOperator.NoBinaryOperator &&
                   (operatorPriority = binaryOperatorPriorityTable[binaryOperator]) < priority)
            {
                var subExprDesc = ExpressionDescription.Empty;
                CodeGenerator.ExpressionInterProcessing(binaryOperator, ref exprDesc, token!);
                Tokenizer.NextWithIgnoreWhiteSpace();

                SubExpression(out var nextOperator, ref subExprDesc, operatorPriority);

                // SubExpression의 재귀가 끝나면 식을 코드화함
                CodeGenerator.ExpressionPostProcessing(binaryOperator, ref exprDesc, ref subExprDesc, token!);
                // 처리되지 않은 오퍼레이터를 받아 다시 처리 시작
                binaryOperator = nextOperator;
            }
        }

        private Token? GetOperatorToken()
        {
            var token = Tokenizer.Current();
            if (token == null)
                return null;

            bool isOperator = token.Type == TokenType.Operator;

            switch (token.Type)
            {
                case TokenType.Error:
                case TokenType.UnexpectedToken:
                    throw new SyntaxErrorException(token);
            }

            return isOperator ? token : null;
        }

        private UnaryOperator CheckUnaryOperator(Token? token)
        {
            return token?.Value switch
            {
                "not" => UnaryOperator.Not,
                "-" => UnaryOperator.Minus,
                "+" => UnaryOperator.Plus,
                _ => UnaryOperator.NoUnaryOperator
            };
        }

        private BinaryOperator CheckBinaryOperator(Token? token)
        {
            return token?.Value switch
            {
                // 수식 연산자
                "+" => BinaryOperator.Add,
                "-" => BinaryOperator.Subtract,
                "*" => BinaryOperator.Multiply,
                "/" => BinaryOperator.Divide,
                "%" => BinaryOperator.Modulo,
                // 비교 연산자
                "==" => BinaryOperator.Equal,
                "!=" => BinaryOperator.NotEqual,
                ">=" => BinaryOperator.GreaterThanOrEqual,
                "<=" => BinaryOperator.LessThanOrEqual,
                ">" => BinaryOperator.GreaterThan,
                "<" => BinaryOperator.LessThan,
                // 논리 연산자
                "&&" => BinaryOperator.And,
                "||" => BinaryOperator.Or,
                "and" => BinaryOperator.And,
                "or" => BinaryOperator.Or,

                _ => BinaryOperator.NoBinaryOperator
            };
        }
    }
}