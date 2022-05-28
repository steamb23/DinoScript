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
        int GetIndentCount(out Token? startToken)
        {
            int count = 0;
            startToken = Tokenizer.Current();

            while (Tokenizer.Next()?.Type == TokenType.WhiteSpace)
            {
                count += 1;
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

        int GetIndentationDifference(in IndentationState indentationState, out int indentCount, out Token? startToken)
        {
            indentCount = GetIndentCount(out startToken);
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
            if (!(token is { Type: TokenType.EndOfLine }))
            {
                throw new SyntaxErrorException(token);
            }
        }

        public void Statement(ref IndentationState indentationState, ref FunctionState funcState)
        {
            if (GetIndentationDifference(in indentationState, out var indentCount, out var token) != 0)
                throw new IndentationException(token, indentCount);
            if (token == null)
            {
                // 프로그램 끝
                return;
            }

            switch (token.Type)
            {
                case TokenType.Keyword:
                    switch (token.Value)
                    {
                        case "let":
                            AssignStatement(ref funcState);
                            break;
                    }
                    break;
                case TokenType.Identifier:
                    AssignStatement(ref funcState);
                    break;
            }

            CheckEndOfLine();
        }

        /// <summary>
        /// 할당문을 파싱합니다.
        /// </summary>
        public void AssignStatement(ref FunctionState funcState)
        {
            AssignExpression(funcState, true);
        }
    }
}