using System;
using System.Collections.Generic;
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
            SubExpression(ref expressionDescription, uint.MaxValue);
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

        private void PrimaryExpression(ref ExpressionDescription expressionDescription, bool stackPush)
        {
            PreAccessExpression(ref expressionDescription, stackPush);
        }

        void PreAccessExpression(ref ExpressionDescription expressionDescription, bool stackPush)
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

            PostAccessExpression(ref expressionDescription, stackPush);
        }

        void PostAccessExpression(ref ExpressionDescription expressionDescription, bool stackPush)
        {
            var token = Tokenizer.Current();

            switch (token?.Type)
            {
                case TokenType.Identifier:
                {
                    // 심볼 테이블에 존재하는지 체크
                    if (token.Value != null && !CurrentFunctionState.SymbolTable.ContainsKey(token.Value))
                        throw new SyntaxErrorException(token, $"'{token.Value}' symbol does not exist.");

                    expressionDescription = CodeGenerator.ExpressionInitialize(
                        ExpressionKind.LocalVariable,
                        0,
                        token,
                        stackPush);
                    return;
                }
                case TokenType.NumberLiteral:
                {
                    if (long.TryParse(token.Value!, out var longValue))
                    {
                        expressionDescription = CodeGenerator.ExpressionInitialize(
                            ExpressionKind.ConstantInteger,
                            longValue,
                            token,
                            stackPush);
                    }
                    else
                    {
                        expressionDescription = CodeGenerator.ExpressionInitialize(
                            ExpressionKind.ConstantNumber,
                            double.Parse(token.Value!),
                            token,
                            stackPush);
                    }

                    return;
                }
                case TokenType.BooleanLiteral:
                {
                    expressionDescription = CodeGenerator.ExpressionInitialize(
                        ExpressionKind.ConstantBoolean,
                        bool.Parse(token.Value!),
                        token,
                        stackPush);
                    return;
                }
            }

            throw new SyntaxErrorException(token);
        }

        #endregion

        private readonly IReadOnlyDictionary<BinaryOperator, uint> binaryOperatorPriorityTable =
            new Dictionary<BinaryOperator, uint>()
            {
                [BinaryOperator.Multiply] = (uint)OperationTypes.Multiplicative,
                [BinaryOperator.Divide] = (uint)OperationTypes.Multiplicative,
                [BinaryOperator.Modulo] = (uint)OperationTypes.Multiplicative,

                [BinaryOperator.Add] = (uint)OperationTypes.Addictive,
                [BinaryOperator.Subtract] = (uint)OperationTypes.Addictive,

                [BinaryOperator.Equal] = (uint)OperationTypes.Equality,
                [BinaryOperator.NotEqual] = (uint)OperationTypes.Equality,

                [BinaryOperator.GreaterThanOrEqual] = (uint)OperationTypes.Comparison,
                [BinaryOperator.LessThanOrEqual] = (uint)OperationTypes.Comparison,
                [BinaryOperator.GreaterThan] = (uint)OperationTypes.Comparison,
                [BinaryOperator.LessThan] = (uint)OperationTypes.Comparison,

                [BinaryOperator.And] = (uint)OperationTypes.LogicalAnd,
                [BinaryOperator.Or] = (uint)OperationTypes.LogicalOr,
            };

        /// <summary>
        /// 우선 순위에 의한 하위 표현식 구조를 표현합니다.
        /// </summary>
        /// <param name="binaryOperator">함수 내에서 처리되지 않은 연산자를 반환합니다.</param>
        /// <param name="exprDesc">식의 설명자입니다.</param>
        /// <param name="priority">연산자의 우선순위입니다. 값이 작을 수록 높은 연산 우선 순위를 나타냅니다.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private BinaryOperator SubExpression(
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
                SubExpression(ref exprDesc, (uint)OperationTypes.Unary);
                CodeGenerator.ExpressionPreProcessing(unaryOperator, ref exprDesc, token!);
            }
            else
            {
                PrimaryExpression(ref exprDesc, true);
            }

            token = Tokenizer.NextWithIgnoreWhiteSpace();
            var binaryOperator = CheckBinaryOperator(token);
            // 확인된 이항 연산자 우선순위가 현재 표현식 우선순위 보다 높으면 재귀 처리
            // 토큰이 연산자가 아닐경우 GetNextBinaryOperatorToken에 의해 null 임
            uint operatorPriority;
            while (binaryOperator != BinaryOperator.NoBinaryOperator &&
                   (operatorPriority = binaryOperatorPriorityTable[binaryOperator]) < priority)
            {
                var subExprDesc = ExpressionDescription.Empty;
                CodeGenerator.ExpressionInterProcessing(binaryOperator, ref exprDesc, token!);
                Tokenizer.NextWithIgnoreWhiteSpace();

                var nextOperator = SubExpression(ref subExprDesc, operatorPriority);

                // SubExpression의 재귀가 끝나면 식을 코드화함
                CodeGenerator.ExpressionPostProcessing(binaryOperator, ref exprDesc, ref subExprDesc, token!);
                // 처리되지 않은 오퍼레이터를 받아 다시 처리 시작
                binaryOperator = nextOperator;
            }

            return binaryOperator;
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
                "!" => UnaryOperator.Not,
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

        /// <summary>
        /// 할당을 포함하는 식을 파싱합니다. 최상위 식으로 볼 수 있습니다.
        /// </summary>
        public void AssignExpression(in FunctionState funcState, bool canNewLocal = false)
        {
            // <Identifier> = <Expression>

            var token = Tokenizer.Current();

            if (token == null)
                return;


            bool newLocal = false;
            //Token? identifierToken;
            LocalSymbolDescription symbolDesc;

            ExpressionKind exprKind;

            switch (token.Type)
            {
                case TokenType.Keyword when canNewLocal && token.Value == "let":
                    newLocal = true;
                    token = Tokenizer.NextWithIgnoreWhiteSpace();
                    if (token is { Type: TokenType.Identifier })
                    {
                        // 공간 확보
                        // TODO: LocalSymbolDescription에 대한 세부 데이터 필요
                        funcState.SymbolTable.Add(token.Value!, new LocalSymbolDescription(0, 0));
                        goto case TokenType.Identifier;
                    }
                    else
                        throw new SyntaxErrorException(token);

                case TokenType.Identifier:
                    if (token.Value == null)
                        throw new SyntaxErrorException(token);
                    if (funcState.SymbolTable.TryGetValue(token.Value, out symbolDesc))
                        exprKind = ExpressionKind.LocalVariable;
                    else if (RootFunctionState.SymbolTable.TryGetValue(token.Value, out symbolDesc))
                        exprKind = ExpressionKind.GlobalVariable;
                    else
                        throw new SyntaxErrorException(token, $"Could not find symbol '{token.Text}'.");
                    break;

                default:
                    throw new SyntaxErrorException(token);
            }

            token = Tokenizer.NextWithIgnoreWhiteSpace();
            if (!(token is { Type: TokenType.Operator, Value: "=" }))
            {
                throw new SyntaxErrorException(token);
            }

            Tokenizer.NextWithIgnoreWhiteSpace();
            var subExprDesc = ExpressionDescription.Empty;

            // 식 코드 생성
            Expression(ref subExprDesc);

            // 할당 코드 생성
            var exprDesc = new ExpressionDescription(exprKind, symbolDesc.LocalIndex);
            CodeGenerator.Assign(ref exprDesc, newLocal, token);
        }
    }
}