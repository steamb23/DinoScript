using System;
using System.Collections.Generic;
using DinoScript.Code;
using DinoScript.Syntax;

namespace DinoScript.Parser
{
    // SyntaxParser_Expressions
    public partial class SyntaxParser
    {
        // 액션 메서드의 반환값은 bool형으로,
        // 해당 액션에 맞는 구문 노드가 생성되었을 경우 true,
        // 그렇지 않을 경우 구문 오류를 뜻하는 false를 반환합니다.

        void Expression()
        {
            SubExpression(out _);
            // 표현식 코드 생성이 안됬을 경우 강제 생성 (PrimaryExpression만 있고 연산자가 없는 경우)
            CodeGenerator.GenerateExpression();
        }

        void GroupExpression()
        {
            var token = Tokenizer.Current();

            if (token?.Value == "(")
            {
                // 토큰 넘어가기
                Tokenizer.NextWithIgnoreWhiteSpace();
                Expression();
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

        void PrimaryExpression()
        {
            // PreAccessExpression{ . PostAccessExpression}
            void AccessExpression()
            {
                var token = Tokenizer.Current();

                void PreAccessExpression()
                {
                    // <Identity>
                    switch (token?.Type)
                    {
                        case TokenType.Mark:
                        {
                            GroupExpression();
                            return;
                        }
                    }

                    PostAccessExpression();
                }

                void PostAccessExpression()
                {
                    switch (token?.Type)
                    {
                        case TokenType.Identifier:
                        case TokenType.NumberLiteral:
                        {
                            if (token.Type == TokenType.Identifier)
                            {
                                // 심볼 테이블에 존재하는지 체크
                                if (token.Value != null && !SymbolTable.ContainsKey(token.Value))
                                    throw new SyntaxErrorException(token, $"'{token.Value}' symbol does not exist.");
                            }

                            // 토큰에 해당하는 값을 코드 생성 큐에 추가함
                            CodeGenerator.AccessTokenEnqueue(token);
                            return;
                        }
                    }

                    throw new SyntaxErrorException(token);
                }

                PreAccessExpression();
            } // end of AccessExpression

            void PostfixIncrementExpression()
            {
                // 후위 증감 표현
                AccessExpression();
                var token = Tokenizer.NextWithIgnoreWhiteSpace();
                switch (token?.Value)
                {
                    case "++":
                    case "--":
                        CodeGenerator.PostfixUnaryTokenEnqueue(token);
                        Tokenizer.NextWithIgnoreWhiteSpace();
                        return;
                    default:
                        return;
                }
            }

            // <AccessExpression>
            AccessExpression();
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
        void SubExpression(out Token? binaryOperator, uint? priority = uint.MaxValue)
        {
            // (<PrimaryExpression>|<UnaryExpression>){ BinaryOperator SubExpression}

            // TODO: UnaryExpression에 대한 코드 추가 필요 (Lua스크립트의 lparser 코드 참조)
            PrimaryExpression();

            binaryOperator = GetBinaryOperatorToken();

            // 확인된 이항 연산자 우선순위가 현재 표현식 우선순위 보다 높으면 재귀 처리
            // 토큰이 연산자가 아닐경우 GetNextBinaryOperatorToken에 의해 null 임
            while (binaryOperator?.Value != null &&
                   binaryOperatorPriorityTable[binaryOperator.Value] <= priority)
            {
                // 연산자를 코드 생성 스택에 우선 푸시해둠.
                CodeGenerator.OperatorTokenPush(binaryOperator);
                Tokenizer.NextWithIgnoreWhiteSpace();

                SubExpression(out var nextOperator, binaryOperatorPriorityTable[binaryOperator.Value]);

                // SubExpression의 재귀가 끝나면 식을 코드화함
                CodeGenerator.GenerateExpression();
                // 처리되지 않은 오퍼레이터를 받아 다시 처리 시작
                binaryOperator = nextOperator;
            }
        }

        Token? GetOperatorToken()
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

        Token? GetUnaryOperatorToken()
        {
            // 전위 단항 연산자를 나타냅니다.
            var operatorToken = GetOperatorToken();
            switch (operatorToken?.Value)
            {
                case "--":
                case "++":
                case "+":
                case "-":
                case "!":
                    return operatorToken;
                default:
                    return null;
            }
        }

        Token? GetBinaryOperatorToken()
        {
            return GetOperatorToken();
        }
    }
}