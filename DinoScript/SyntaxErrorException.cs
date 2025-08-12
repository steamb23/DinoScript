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

using DinoScript.Lexer;

namespace DinoScript;

/// <summary>
/// 구문 분석 중 발생하는 구문 오류에 대한 예외입니다.
/// </summary>
public class SyntaxErrorException(Token? token, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>
    /// 지정된 토큰으로 <see cref="SyntaxErrorException"/> 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="token">구문 오류를 발생시킨 토큰입니다.</param>
    public SyntaxErrorException(Token? token)
        : this(token, token == null
            ? "No token."
            : $"'{token.Value.RawText}' is invalid token.")
    {
        Token = token;
    }

    /// <summary>
    /// 현재 예외를 설명하는 메시지를 가져옵니다. 토큰이 있는 경우 토큰의 위치 정보를 포함합니다.
    /// </summary>
    public override string Message =>
        Token == null ? base.Message : $"({Token.Value.Lines}, {Token.Value.Columns}) : {base.Message}";

    /// <summary>
    /// 구문 오류를 발생시킨 토큰을 가져옵니다.
    /// </summary>
    public Token? Token { get; } = token;
}