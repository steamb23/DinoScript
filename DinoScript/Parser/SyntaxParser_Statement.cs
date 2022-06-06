using System;
using System.Collections.Generic;
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
        void CheckEndOfLineAndSemicoln()
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

        void CheckEndOfLine()
        {
            var token = Tokenizer.Current();
            if (!(token is { Type: TokenType.EndOfLine }))
                throw new SyntaxErrorException(token);
            Tokenizer.Next();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indentationState"></param>
        /// <param name="funcState"></param>
        /// <param name="addIndent"></param>
        /// <returns></returns>
        /// <exception cref="IndentationException"></exception>
        private void StatementList(
            IndentationState indentationState,
            in FunctionState funcState,
            bool addIndent,
            out List<int>? breakList)
        {
            int indentDiff;
            int indentCount;
            Token? indentToken;
            Token? token;

            breakList = null;

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
                Statement(indentationState, funcState, out _);
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
                Statement(indentationState, funcState, out breakList);
            }
        }

        private void Statement(in IndentationState indentationState, in FunctionState funcState, out List<int>? breakList)
        {
            var token = Tokenizer.Current();
            breakList = null;

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
                            IfStatement(indentationState, funcState, out breakList);
                            break;
                        case "while":
                            WhileStatement(indentationState, funcState);
                            break;
                        case "break":
                            BreakStatement(out breakList);
                            break;
                    }

                    break;
                case TokenType.Identifier:
                    AssignStatement(funcState);
                    break;
            }

            CheckEndOfLineAndSemicoln();
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
            in Token? ifToken, out List<int>? breakList)
        {
            var exprDesc = ExpressionDescription.Empty;

            Tokenizer.NextWithIgnoreWhiteSpace();
            AssignExpression(funcState, ref exprDesc, false, true);

            CheckEndOfLine();

            var branchPosition = CodeGenerator.IfNotBranch(ifToken);
            StatementList(indentationState, funcState, true, out breakList);
            // if 문 탈출
            escapeChain = CodeGenerator.IfEscape(ifToken, escapeChain);
            // if 조건 판정 실패시 넘겨질 위치
            CodeGenerator.FixBranchToHere(branchPosition);
            return branchPosition;
        }

        private void IfStatement(in IndentationState indentationState, in FunctionState funcState, out List<int>? breakList)
        {
            var ifToken = Tokenizer.Current();
            int escapeChain = CodeGenerator.NoJump;
            var ifNotBranchPos = IfElseChain(indentationState, funcState, ref escapeChain, ifToken, out breakList); // if <cond> ..

            bool isElse;
            while ((isElse = Tokenizer.Current() is { Value: "else" }) &&
                   (ifToken = Tokenizer.NextWithIgnoreWhiteSpace()) is { Value: "if" })
            {
                IfElseChain(indentationState, funcState, ref escapeChain, ifToken, out var breakList2); // else if <cond> ..
                breakList =  CodeGenerator.ConcatBreakList(breakList, breakList2);
            }

            // else로 끝남
            bool isElseEnd = false;
            if (isElse)
            {
                Tokenizer.Next();
                isElseEnd = true;
                StatementList(indentationState, funcState, true, out var breakList2); // else ..
                breakList =  CodeGenerator.ConcatBreakList(breakList, breakList2);
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

        private void WhileStatement(in IndentationState indentationState, in FunctionState funcState)
        {
            var exprDesc = ExpressionDescription.Empty;
            var whileToken = Tokenizer.Current();

            Tokenizer.NextWithIgnoreWhiteSpace();
            AssignExpression(funcState, ref exprDesc, false, true);

            var branchPos = CodeGenerator.IfNotBranch(whileToken);

            CheckEndOfLine();

            StatementList(indentationState, funcState, true, out var breakList);

            CodeGenerator.BreakListPatchToHere(breakList);
            CodeGenerator.FixBranchToHere(branchPos);
        }

        private void BreakStatement(out List<int> breakList)
        {
            var token = Tokenizer.Current();
            CodeGenerator.Break(token);
            breakList = new List<int> { 1 };
        }
    }
}