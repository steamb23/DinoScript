using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DinoScript.Syntax;

namespace DinoScript.Parser
{
    /// <summary>
    /// 토크나이저를 나타냅니다.
    /// </summary>
    public class Tokenizer : IDisposable
    {
        private TextBuffer textBuffer;

        private Token? currentToken;

        private int lines = 0;
        private int columns = 0;

        private readonly TokenDefinition endOfLineTokenDefinition = new TokenDefinition(TokenType.EndOfLine,
            "^\r?\n|\r|\u0085|\u2028|\u2029");

        // Regex 문법 테스트시 http://regexstorm.net/tester 참조
        // 주의: EndOfLine과 StringLiteral을 제외한 나머지 토큰은 토큰 내에 개행이 올 수 없습니다.
        private static readonly List<TokenDefinition> tokenDefinitions = new List<TokenDefinition>
        {
            // Regex 지옥... 죽여줘...
            // // 문자열 리터럴은 수동 처리
            // new(TokenType.StringLiteral,
            //     // "{singleCharacter}"
            //     "^\"[^\"]*\""),
            new TokenDefinition(TokenType.WhiteSpace,
                "^(?: +|\t+|\u3000+|\\\\(?:\r?\n|\r|\u0085|\u2028|\u2029))"),
            new TokenDefinition(TokenType.EndOfLine,
                "^(?:\r?\n|\r|\u0085|\u2028|\u2029)"),
            new TokenDefinition(TokenType.Keyword,
                "^(?:func|for|in|if|else|var|do|until|while|not|this|get|set)"),
            new TokenDefinition(TokenType.Operator,
                "^(?:\\+|-|\\*|/|%|\\^|==|!=|<=|>=|<|>|\\?|:|\\+\\+|--|!|&&|\\|\\||and|or)"),
            new TokenDefinition(TokenType.Mark,
                "^(?:\\(|\\)|\\[|\\])"),
            new TokenDefinition(TokenType.NumberLiteral,
                "^(?:0x[0-9a-fA-F_]+|0b[01_]+|[0-9_]+\\.?[0-9_]*)"),
            new TokenDefinition(TokenType.CharacterLiteral,
                // 'singleCharacter'
                "^(?:'[^']?)'"),
            new TokenDefinition(TokenType.BooleanLiteral,
                "^(?:true|false)"),
            new TokenDefinition(TokenType.NullLiteral,
                "^(?:null)"),
            new TokenDefinition(TokenType.UndefinedLiteral,
                "^(?:undefined)"),
            new TokenDefinition(TokenType.Identifier,
                // letter{letter|decimalDigit}
                "^(?:\\p{Lu}|\\p{Ll}|\\p{Lt}|\\p{Lm}|\\p{Lo}|\\p{Nl})(\\p{Lu}|\\p{Ll}|\\p{Lt}|\\p{Lm}|\\p{Lo}|\\p{Nl}|\\d)*"),
        };

        // 규격 외 공백 문자 제거
        private readonly List<Regex> skipRegexes = new List<Regex>
        {
            new Regex("^(\\s)+")
        };

        public Tokenizer(TextReader textReader)
        {
            this.textBuffer = new TextBuffer(textReader);
        }

        public bool IsEndOfText => textBuffer.IsEndOfText;

        /// <summary>
        /// 현재 토큰을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public Token? Current()
        {
            // 현재 토큰이 없을 경우 다음 토큰을 가져오는 것을 시도
            if (currentToken == null)
                return Next();
            return currentToken;
        }

        /// <summary>
        /// 다음 토큰을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public Token? Next()
        {
            var text = textBuffer.GetText();

            // 문자열의 끝
            if (string.IsNullOrEmpty(text))
            {
                return currentToken = null;
            }

            // 줄 및 열 백업
            // 이 데이터는 현재 토큰의 위치 값으로 사용
            var currentLines = lines;
            var currentColumns = columns;

            #region 특수 처리

            if (text[0] == '\"')
            {
                return StringLiteralProcess(text);
            }

            #endregion

            foreach (var tokenDefinition in tokenDefinitions)
            {
                var match = tokenDefinition.Regex.Match(text);
                if (match.Success)
                {
                    // 내부 데이터 관련 연산
                    switch (tokenDefinition.Type)
                    {
                        case TokenType.WhiteSpace:
                            break;
                        case TokenType.EndOfLine:
                            // 개행 처리
                            lines++;
                            columns = 0;
                            break;
                        default:
                            // 처리된 문자열 길이 만큼 컬럼 수 가산
                            columns += match.Value.Length;
                            break;
                    }

                    textBuffer.Cutout(match.Value.Length);
                    return currentToken = MakeToken(tokenDefinition.Type, match.Value, currentLines, currentColumns);
                }
            }

            // 규격 외 문자 스킵 후 재시도
            foreach (var regex in skipRegexes)
            {
                var match = regex.Match(text);
                if (match.Success)
                {
                    textBuffer.Cutout(match.Length);
                    return Next();
                }
            }

            // 알 수 없는 토큰
            currentToken = MakeToken(TokenType.UnexpectedToken, text[0].ToString(), currentLines, currentColumns);
            throw new SyntaxErrorException(currentToken);
        }

        /// <summary>
        /// 공백을 제외한 다음 토큰을 가져옵니다.
        /// </summary>
        /// <returns></returns>
        public Token? NextWithIgnoreWhiteSpace()
        {
            Token? token;

            while ((token = Next())?.Type == TokenType.WhiteSpace)
            {
                // 공백 제거
            }

            return token;
        }

        /// <summary>
        /// 문자열 리터럴에 대한 토큰화 처리를 진행합니다.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Token StringLiteralProcess(string text)
        {
            var builder = new StringBuilder();

            // 현재 줄 및 컬럼 저장
            var currentLines = this.lines;
            var currentColumns = this.columns;

            // 반복 횟수
            var iterations = 0;

            var quotationMarkFind = 0;
            // 다음 따옴표 검색
            while (!string.IsNullOrEmpty(text))
            {
                iterations++;

                var textLength = text.Length;
                for (int i = 0; i < textLength; i++)
                {
                    // 컬럼 가산
                    columns++;

                    builder.Append(text[i]);
                    if (text[i] == '\"')
                    {
                        // 이스케이프 문자면 넘김
                        if (i > 0 && text[i - 1] == '\\')
                        {
                            continue;
                        }

                        // 개행 처리
                        if (endOfLineTokenDefinition.Regex.Match(text, i).Success)
                        {
                            lines++;
                            columns = 0;
                        }

                        quotationMarkFind++;

                        // 두번 찾았으면 토큰 리턴
                        if (quotationMarkFind >= 2)
                        {
                            textBuffer.Cutout(columns);
                            return MakeToken(TokenType.StringLiteral, builder.ToString(), currentLines, currentColumns);
                        }
                    }
                }

                // 텍스트 새로 받아오기
                text = textBuffer.NextText();
            }

            // 따옴표 찾는데 실패함.
            return MakeToken(TokenType.Error, builder.ToString(), currentLines, currentColumns);
        }

        private Token MakeToken(TokenType tokenType, string text, long tokeLines, long tokenColumns, string? message = null)
        {
            // 일부 토큰에 대한 가공 처리
            switch (tokenType)
            {
                case TokenType.StringLiteral:
                case TokenType.CharacterLiteral:
                    // 앞뒤 문자(따옴표)제거
                    return InternalMakeToken(text.Substring(1, text.Length - 2));
                case TokenType.NumberLiteral:
                    // _기호 제거
                    return InternalMakeToken(text.Replace("_", ""));
                default:
                    return InternalMakeToken(text);
            }

            Token InternalMakeToken(string? value)
            {
                return new Token(
                    type: tokenType,
                    text: text,
                    value: value,
                    lines: tokeLines + 1,
                    columns: tokenColumns + 1
                );
                // {
                //     Type = tokenType,
                //     Value = value,
                //     Text = text,
                //     Lines = tokeLines + 1,
                //     Columns = tokenColumns + 1
                // };
            }
        }

        public void Dispose()
        {
            textBuffer.Dispose();
        }
    }
}
