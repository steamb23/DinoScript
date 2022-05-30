using System;
using DinoScript.Syntax;

namespace DinoScript.Parser
{
    // SyntaxParser_Statement
    public partial class SyntaxParser
    {
        /// <summary>
        /// 다음 공백 토큰의 갯수를 측정하고 스킵합니다. 토크나이저 상태가 변경될 수 있습니다.
        /// </summary>
        /// <returns></returns>
        int GetIndentCount(out Token? startToken, out Token? skippedToken)
        {
            int count = 0;
            startToken = Tokenizer.Current();
            skippedToken = startToken;

            while (skippedToken?.Type == TokenType.WhiteSpace)
            {
                count += 1;
                skippedToken = Tokenizer.Next();
            }

            return count;
        }

        /// <summary>
        /// 들여쓰기 상태 객체와 indentCount의 값을 비교합니다.
        /// </summary>
        /// <param name="sourceIndentCount"></param>
        /// <param name="destIndentCount"></param>
        /// <returns>값이 일치할 경우 0, <see cref="destIndentCount"/>가 더 크면 양의 정수, 작으면 음의 정수를 반환합니다.</returns>
        int GetIndentationDifference(int sourceIndentCount, int destIndentCount) =>
            destIndentCount - sourceIndentCount;

        int GetIndentationDifference(in IndentationState indentationState, out int indentCount, out Token? startToken, out Token? skippedToken)
        {
            if ((startToken = Tokenizer.Current()) is { Type: TokenType.Semicolon })
            {
                skippedToken = Tokenizer.NextWithIgnoreWhiteSpace();
                indentCount = indentationState.IndentCount;
                return 0;
            }

            indentCount = GetIndentCount(out startToken, out skippedToken);
            var indentDiff = GetIndentationDifference(indentationState.IndentCount, indentCount);
            return indentDiff;
        }

        /// <summary>
        /// 줄바꿈 문자가 있는지 체크합니다. 아닐 경우 예외를 발생시킵니다.
        /// </summary>
        /// <exception cref="SyntaxErrorException"></exception>
        void CheckEndOfLine()
        {
            var token = Tokenizer.Current();
            if (token != null && token.Type != TokenType.EndOfLine)
            {
                if (token.Type == TokenType.Semicolon)
                {
                    return;
                }

                throw new SyntaxErrorException(token);
            }

            Tokenizer.Next();
        }

        public void StatementList(in IndentationState indentationState, in FunctionState funcState)
        {
            while (Tokenizer.Current() != null)
            {
                Statement(indentationState, funcState);
            }
        }

        public void Statement(in IndentationState indentationState, in FunctionState funcState)
        {
            if (GetIndentationDifference(in indentationState, out var indentCount,
                    out var indentToken, out var token) != 0)
                throw new IndentationException(indentToken, indentCount);
            if (token == null)
            {
                // 프로그램 끝
                return;
            }

            if (token.Type == TokenType.Semicolon)
            {
                token = Tokenizer.NextWithIgnoreWhiteSpace();
            }

            switch (token.Type)
            {
                case TokenType.Keyword:
                    switch (token.Value)
                    {
                        case "let":
                            AssignStatement(funcState);
                            break;
                    }

                    break;
                case TokenType.Identifier:
                    AssignStatement(funcState);
                    break;
            }

            CheckEndOfLine();
        }

        /// <summary>
        /// 할당문을 파싱합니다.
        /// </summary>
        public void AssignStatement(in FunctionState funcState)
        {
            AssignExpression(funcState, true);
        }
    }
}