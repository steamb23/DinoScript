#region License

// Copyright (c) 2025 Choi Jiheon (steamb23@outlook.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Text.RegularExpressions;

// ReSharper disable MemberCanBePrivate.Global

namespace DinoScript.Parser.Syntax;

/// <summary>
/// DinoScript 언어의 토큰화에 사용되는 정규식들을 정의합니다.
/// </summary>
internal static partial class Regexes
{
    /// <summary>
    /// 주어진 토큰 타입에 해당하는 정규식을 반환합니다.
    /// </summary>
    /// <param name="type">정규식을 가져올 토큰의 타입입니다.</param>
    /// <returns>해당 토큰 타입을 인식하기 위한 정규식입니다.</returns>
    /// <exception cref="ArgumentOutOfRangeException">지원하지 않는 토큰 타입이 지정된 경우 발생합니다.</exception>
    public static Regex GetRegex(TokenType type)
    {
        switch (type)
        {
            case TokenType.Whitespace:
                return WhitespaceRegex();
            case TokenType.Semicolon:
                return SemicolonRegex();
            case TokenType.EndOfLine:
                return EndOfLineRegex();
            case TokenType.Keyword:
                return KeywordRegex();
            case TokenType.Operator:
                return OperatorRegex();
            case TokenType.Mark:
                return MarkRegex();
            case TokenType.BooleanLiteral:
                return BooleanLiteralRegex();
            case TokenType.NumberLiteral:
                return NumberLiteralRegex();
            case TokenType.CharacterLiteral:
                return CharacterLiteralRegex();
            case TokenType.NullLiteral:
                return NullLiteralRegex();
            case TokenType.UndefinedLiteral:
                return UndefinedLiteralRegex();
            case TokenType.Identifier:
                return IdentifierRegex();
            // 아래 토큰 타입은 Regex로 처리되지 않음
            case TokenType.Error:
            case TokenType.UnexpectedToken:
            case TokenType.StringLiteral: // 문자열 리터럴은 수동 처리
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    /// <summary>
    /// 공백 문자를 인식하는 정규식입니다.
    /// 스페이스, 탭, 전각 공백, 줄 연속 문자를 인식합니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?: +|\t+|\u3000+|\\(?:\r?\n|\r|\u0085|\u2028|\u2029))
                    """, RegexOptions.Singleline)]
    public static partial Regex WhitespaceRegex();

    /// <summary>
    /// 세미콜론(;)을 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:;)
                    """, RegexOptions.Singleline)]
    public static partial Regex SemicolonRegex();

    /// <summary>
    /// 줄 끝을 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:\r?\n|\r|\u0085|\u2028|\u2029)
                    """, RegexOptions.Singleline)]
    public static partial Regex EndOfLineRegex();

    /// <summary>
    /// 키워드를 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:let|func|for|each|in|if|else|var|do|until|while|not|this|get|set)
                    """,
        RegexOptions.Singleline)]
    public static partial Regex KeywordRegex();

    /// <summary>
    /// 연산자를 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:\+|-|\*|/|%|\^|==|!=|<=|>=|<|>|\?|:|\+\+|--|!|&&|\|\||and|or|=|\.\.)
                    """,
        RegexOptions.Singleline)]
    public static partial Regex OperatorRegex();

    /// <summary>
    /// 괄호 기호를 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:\(|\)|\[|\])
                    """, RegexOptions.Singleline)]
    public static partial Regex MarkRegex();

    /// <summary>
    /// 숫자 리터럴을 인식하는 정규식입니다.
    /// 16진수, 2진수, 10진수를 지원합니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:0x[0-9a-fA-F_]+|0b[01_]+|[0-9_]+\.?[0-9_]*)
                    """, RegexOptions.Singleline)]
    public static partial Regex NumberLiteralRegex();

    /// <summary>
    /// 문자 리터럴을 인식하는 정규식입니다.
    /// 작은따옴표로 둘러싸인 단일 문자를 인식합니다.
    /// </summary>
    // 'singleCharacter'
    [GeneratedRegex("""
                    ^(?:'[^']?)'
                    """, RegexOptions.Singleline)]
    public static partial Regex CharacterLiteralRegex();

    /// <summary>
    /// 불리언 리터럴을 인식하는 정규식입니다.
    /// true와 false 값을 인식합니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:true|false)
                    """, RegexOptions.Singleline)]
    public static partial Regex BooleanLiteralRegex();

    /// <summary>
    /// null 리터럴을 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:null)
                    """, RegexOptions.Singleline)]
    public static partial Regex NullLiteralRegex();

    /// <summary>
    /// undefined 리터럴을 인식하는 정규식입니다.
    /// </summary>
    [GeneratedRegex("""
                    ^(?:undefined)
                    """, RegexOptions.Singleline)]
    public static partial Regex UndefinedLiteralRegex();

    /// <summary>
    /// 식별자를 인식하는 정규식입니다.
    /// 유니코드 문자와 숫자로 구성된 식별자를 인식합니다.
    /// </summary>
    // letter{letter|decimalDigit}
    [GeneratedRegex("""
                    ^(?:\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl})(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}|\d)*
                    """,
        RegexOptions.Singleline)]
    public static partial Regex IdentifierRegex();

    /// <summary>
    /// 문자열 리터럴을 인식하는 정규식입니다.
    /// 큰따옴표로 둘러싸인 문자열을 인식합니다.
    /// </summary>
    [GeneratedRegex("""
                    "(?:\\.|[^"\\])*"
                    """
        , RegexOptions.Singleline)]
    public static partial Regex StringLiteralRegex();
}