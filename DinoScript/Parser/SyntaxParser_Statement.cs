using System;
using DinoScript.Code;
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

        int GetIndentationDifference(in IndentationState indentationState, out int indentCount, out Token? startToken,
            out Token? skippedToken)
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
                    Tokenizer.NextWithIgnoreWhiteSpace();
                    return;
                }

                throw new SyntaxErrorException(token);
            }

            Tokenizer.Next();
        }

        private void StatementList(IndentationState indentationState, in FunctionState funcState,
            bool addIndent)
        {
            int indentDiff;
            int indentCount;
            Token? indentToken;
            Token? token;

            if (addIndent && Tokenizer.Current() != null)
            {
                indentDiff = GetIndentationDifference(
                    in indentationState,
                    out indentCount,
                    out indentToken,
                    out token);

                if (indentDiff > 0)
                    indentationState = new IndentationState(
                        indentCount,
                        indentationState.Depth + 1);
                else
                    throw new IndentationException(indentToken, indentCount);
                Statement(indentationState, funcState);
            }

            while (Tokenizer.Current() != null)
            {
                indentDiff = GetIndentationDifference(
                    in indentationState,
                    out indentCount,
                    out indentToken,
                    out token);
                if (indentDiff < 0)
                    // 구문 목록 종료
                    return;
                if (indentDiff != 0)
                    throw new IndentationException(indentToken, indentCount);
                Statement(indentationState, funcState);
            }
        }

        private void Statement(in IndentationState indentationState, in FunctionState funcState)
        {
            var token = Tokenizer.Current();

            if (token == null)
                // 프로그램 끝
                return;

            switch (token.Type)
            {
                case TokenType.Keyword:
                    switch (token.Value)
                    {
                        case "let":
                            AssignStatement(funcState);
                            break;
                        case "if":
                            IfStatement(indentationState, funcState);
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
        private void AssignStatement(in FunctionState funcState)
        {
            var exprDesc = ExpressionDescription.Empty;
            AssignExpression(funcState, ref exprDesc, true);
        }

        // break 문 추가 필요
        private int IfElseChain(in IndentationState indentationState, in FunctionState funcState, ref int escapeChain,
            in Token? ifToken)
        {
            var exprDesc = ExpressionDescription.Empty;

            Tokenizer.NextWithIgnoreWhiteSpace();
            AssignExpression(funcState, ref exprDesc, false, true);
            var token = Tokenizer.Current();
            if (!(token is { Type: TokenType.EndOfLine }))
                throw new SyntaxErrorException(token);
            Tokenizer.Next();

            var branchPosition = CodeGenerator.IfNotBranch(token);
            StatementList(indentationState, funcState, true);
            // if 문 탈출
            escapeChain = CodeGenerator.IfEscape(ifToken, escapeChain);
            // if 조건 판정 실패시 넘겨질 위치
            CodeGenerator.FixBranchToHere(branchPosition);
            return branchPosition;
        }

        private void IfStatement(in IndentationState indentationState, in FunctionState funcState)
        {
            var ifToken = Tokenizer.Current();
            int escapeChain = CodeGenerator.NoJump;
            var ifNotBranchPos = IfElseChain(indentationState, funcState, ref escapeChain, ifToken); // if <cond> ..

            bool isElse;
            while ((isElse = Tokenizer.Current() is { Value: "else" }) &&
                   (ifToken = Tokenizer.NextWithIgnoreWhiteSpace()) is { Value: "if" })
            {
                IfElseChain(indentationState, funcState, ref escapeChain, ifToken); // else if <cond> ..
            }

            // else로 끝남
            bool isElseEnd = false;
            if (isElse)
            {
                Tokenizer.Next();
                isElseEnd = true;
                StatementList(indentationState, funcState, true); // else ..
            }

            // 불필요한 브랜치 제거
            if (!isElseEnd)
            {
                CodeGenerator.Codes.RemoveAt(CodeGenerator.Codes.Count - 1);
                CodeGenerator.FixBranchToHere(ifNotBranchPos);
            }
            else
            {
                // 탈출지점
                CodeGenerator.PatchBranchToHere(escapeChain);
            }
        }
    }
}