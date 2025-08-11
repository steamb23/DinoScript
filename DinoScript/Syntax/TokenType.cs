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

namespace DinoScript.Syntax;

/// <summary>
/// 토큰의 타입을 열거합니다.
/// </summary>
public enum TokenType
{
    /// <summary>
    /// 오류가 발생한 토큰의 타입입니다.
    /// </summary>
    Error = -1,

    /// <summary>
    /// 어느 토큰 타입에도 속하지 않는 토큰의 타입입니다.
    /// </summary>
    UnexpectedToken = 0,
    /// <summary>
    /// 공백 문자를 나타내는 토큰의 타입입니다.
    /// </summary>
    Whitespace,
    /// <summary>
    /// 세미콜론(;)을 나타내는 토큰의 타입입니다.
    /// </summary>
    Semicolon,
    /// <summary>
    /// 줄의 끝을 나타내는 토큰의 타입입니다.
    /// </summary>
    EndOfLine,
    /// <summary>
    /// 식별자를 나타내는 토큰의 타입입니다.
    /// </summary>
    Identifier,
    /// <summary>
    /// 키워드를 나타내는 토큰의 타입입니다.
    /// </summary>
    Keyword,
    /// <summary>
    /// 연산자를 나타내는 토큰의 타입입니다.
    /// </summary>
    Operator,
    /// <summary>
    /// 구분자를 나타내는 토큰의 타입입니다.
    /// </summary>
    Mark,
    /// <summary>
    /// 불리언 리터럴을 나타내는 토큰의 타입입니다.
    /// </summary>
    BooleanLiteral,
    /// <summary>
    /// 숫자 리터럴을 나타내는 토큰의 타입입니다.
    /// </summary>
    NumberLiteral,
    /// <summary>
    /// 문자 리터럴을 나타내는 토큰의 타입입니다.
    /// </summary>
    CharacterLiteral,
    /// <summary>
    /// 문자열 리터럴을 나타내는 토큰의 타입입니다.
    /// </summary>
    StringLiteral,
    /// <summary>
    /// null 리터럴을 나타내는 토큰의 타입입니다.
    /// </summary>
    NullLiteral,
    /// <summary>
    /// undefined 리터럴을 나타내는 토큰의 타입입니다.
    /// </summary>
    UndefinedLiteral,
}