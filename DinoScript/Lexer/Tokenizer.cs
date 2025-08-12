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

using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using DinoScript.Internal;

namespace DinoScript.Lexer;

/// <summary>
/// 스크립트 문자열을 토큰화하는 클래스입니다.
/// </summary>
public partial class Tokenizer : IEnumerable<Token>
{
    private readonly string? _rawScript;

    /// <summary>
    /// 현재 처리 중인 줄 번호입니다.
    /// </summary>
    private int _lines;

    /// <summary>
    /// 현재 처리 중인 열 번호입니다.
    /// </summary>
    private int _columns;

    /// <summary>
    /// 처리할 스크립트 버퍼입니다.
    /// </summary>
    private ReadOnlyMemory<char> _scriptBuffer;

    /// <summary>
    /// 줄 끝 토큰 정의입니다.
    /// </summary>
    private static readonly TokenDefinition EndOfLineTokenDefinition = new(TokenType.EndOfLine);

    /// <summary>
    /// 공백 토큰 정의입니다.
    /// </summary>
    private static readonly TokenDefinition WhitespaceTokenDefinition = new(TokenType.Whitespace);

    /// <summary>
    /// 토큰 정의 목록을 나타내며, 토큰화 과정에서 사용할 규칙의 집합입니다.
    /// </summary>
    private static readonly List<TokenDefinition> TokenDefinitions =
    [
        WhitespaceTokenDefinition,
        new(TokenType.Semicolon),
        EndOfLineTokenDefinition,
        new(TokenType.Keyword),
        new(TokenType.Operator),
        new(TokenType.Mark),
        new(TokenType.NumberLiteral),
        new(TokenType.CharacterLiteral),
        new(TokenType.BooleanLiteral),
        new(TokenType.NullLiteral),
        new(TokenType.UndefinedLiteral),
        new(TokenType.Identifier)
    ];

    // 규격 외 공백 문자 제거
    /// <summary>
    /// 스크립트를 토큰화하는 과정에서 특정 규격 외 데이터를 스킵하기 위한 정규식 목록입니다.
    /// </summary>
    private readonly List<Regex> _skipRegexes = [ExtraWhitespaceFilterRegex()];

    /// <summary>
    /// 스크립트 문자열을 토큰화하는 클래스입니다.
    /// </summary>
    /// <param name="rawScript">토큰화할 스크립트 문자열입니다.</param>
    public Tokenizer(string? rawScript)
    {
        ArgumentNullException.ThrowIfNull(rawScript);
        _rawScript = rawScript;
        _scriptBuffer = rawScript.AsMemory();
    }

    /// <summary>
    /// 현재 처리된 토큰입니다.
    /// </summary>
    public Token? CurrentToken { get; private set; }

    /// <summary>
    /// 다음 토큰을 가져옵니다.
    /// </summary>
    /// <returns>다음 토큰입니다. 스크립트의 끝에 도달하면 null을 반환합니다.</returns>
    /// <exception cref="SyntaxErrorException">구문 분석 중 오류가 발생한 경우 발생합니다.</exception>
    ///
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public Token? NextToken()
    {
        var scriptBufferSpan = _scriptBuffer.Span;

        // 문자열의 끝
        if (scriptBufferSpan.Length == 0)
            return CurrentToken = null;

        // 줄 및 열 백업
        // 이 데이터는 현재 토큰의 위치 값으로 사용
        var currentLines = _lines;
        var currentColumns = _columns;

        #region 문자열 특수 파싱

        if (scriptBufferSpan[0] == '\"')
        {
            return CurrentToken = StringLiteralProcess(scriptBufferSpan);
        }

        #endregion

        foreach (var tokenDefinition in TokenDefinitions)
        {
            if (!tokenDefinition.Regex.FirstValueMatch(scriptBufferSpan, out var match))
                continue;
            // 내부 데이터 관련 연산
            switch (tokenDefinition.Type)
            {
                case TokenType.Whitespace:
                    // No Operations
                    break;
                case TokenType.EndOfLine:
                    // 개행 처리
                    _lines++;
                    _columns = 0;
                    break;
                default:
                    // 처리된 문자열 길이 만큼 컬럼 수 가산
                    _columns += match.Length;
                    break;
            }

            var scriptBuffer = _scriptBuffer;
            _scriptBuffer = _scriptBuffer[match.Length..];
            return CurrentToken = MakeToken(
                tokenDefinition.Type,
                scriptBuffer[..match.Length],
                null,
                currentLines,
                currentColumns);
        }


        // 규격 외 문자 스킵 후 재시도
        foreach (var regex in _skipRegexes)
        {
            if (!regex.FirstValueMatch(scriptBufferSpan, out var match)) continue;
            _scriptBuffer = _scriptBuffer[match.Length..];
            return NextToken();
        }

        // 알 수 없는 토큰
        CurrentToken = MakeToken(
            TokenType.UnexpectedToken,
            _scriptBuffer.Length > 0 ? _scriptBuffer[..1] : _scriptBuffer,
            null,
            currentLines,
            currentColumns);
        throw new SyntaxErrorException(CurrentToken);
    }

    /// <summary>
    /// 문자열 리터럴을 처리하여 토큰을 생성합니다.
    /// </summary>
    /// <param name="scriptBufferSpan">문자열 리터럴이 포함된 스크립트의 읽기 전용 범위입니다.</param>
    /// <returns>처리된 문자열 리터럴을 나타내는 토큰입니다.</returns>
    private Token StringLiteralProcess(ReadOnlySpan<char> scriptBufferSpan)
    {
        var builder = ObjectPools.StringBuilderPool.Get();

        try
        {
            return StringLiteralProcess(scriptBufferSpan, builder);
        }
        finally
        {
            ObjectPools.StringBuilderPool.Return(builder);
        }
    }

    /// <summary>
    /// 문자열 리터럴을 처리하여 토큰을 생성하는 메서드입니다.
    /// </summary>
    /// <param name="scriptBufferSpan">입력 스크립트의 현재 처리를 위한 문자 버퍼입니다.</param>
    /// <param name="builder">문자열 리터럴의 가공된 내용을 축적하기 위한 StringBuilder 객체입니다.</param>
    /// <returns>생성된 문자열 리터럴 토큰을 반환합니다. 처리 실패 시에는 오류 토큰을 반환합니다.</returns>
    private Token StringLiteralProcess(ReadOnlySpan<char> scriptBufferSpan, StringBuilder builder)
    {
        // 현재 줄 및 컬럼 저장
        var currentLines = _lines;
        var currentColumns = _columns;

        var quotationMarkFind = 0;

        var skipNextWhitespaces = false;

        for (var i = 0; i < scriptBufferSpan.Length; i++)
        {
            // 컬럼 미리 가산
            _columns++;

            // 쌍따옴표를 찾았을 경우
            if (scriptBufferSpan[i] == '\"')
            {
                // 직전 문자가 이스케이프 문자면 Skip
                if (i > 0 && scriptBufferSpan[i - 1] == '\\')
                {
                    continue;
                }

                // 쌍따옴표 두개를 찾아야 이 로직이 끝남
                if (++quotationMarkFind < 2) continue;

                // 두번 찾았으면 토큰 리턴
                // rawText: 가공처리되지 않은 문자열 리터럴
                // text: 가공처리된 문자열 리터럴
                var rawText = _scriptBuffer[..(i + 1)];
                _scriptBuffer = _scriptBuffer[(i + 1)..];
                return MakeToken(
                    TokenType.StringLiteral,
                    rawText,
                    builder.ToString(),
                    currentLines,
                    currentColumns);
            }

            // 개행 문자 처리
            if (EndOfLineTokenDefinition.Regex.FirstValueMatch(scriptBufferSpan[i..], out var match))
            {
                // 매칭된 개행 문자 빌더에 추가
                builder.Append(scriptBufferSpan[i..(i + match.Length)]);

                i += Math.Max(0, match.Length - 1);

                _lines++;
                _columns = 0;
                // 다음 공백문자들은 무시
                skipNextWhitespaces = true;
                continue;
            }

            // skipNextWhitespaces가 참일 경우
            if (skipNextWhitespaces)
            {
                // Regex에 선언된 Whitespace 문자가 안올때까지 스킵함
                if (i + 1 <= scriptBufferSpan.Length && // i + 1이 Length와 같을 경우 처리가 의미 없어짐...
                    WhitespaceTokenDefinition.Regex.IsMatch(scriptBufferSpan[i..(i + 1)]))
                    continue;
                skipNextWhitespaces = false;
            }

            builder.Append(scriptBufferSpan[i]);
        }

        // 따옴표 찾는데 실패함.
        return MakeToken(
            TokenType.Error,
            _scriptBuffer,
            builder.ToString(),
            currentLines,
            currentColumns);
    }


    /// <summary>
    /// 토크나이저 상태를 초기 상태로 재설정합니다.
    /// </summary>
    public void Reset()
    {
        _scriptBuffer = _rawScript.AsMemory();
    }

    /// <summary>
    /// 주어진 데이터로 새로운 토큰을 생성합니다.
    /// </summary>
    /// <param name="tokenType">생성할 토큰의 타입입니다.</param>
    /// <param name="rawText">토큰의 원시 문자열 데이터입니다.</param>
    /// <param name="text">재정의된 텍스트입니다. null일 경우 특정 타입에 따라 처리됩니다.</param>
    /// <param name="tokeLines">토큰이 위치한 줄 번호입니다.</param>
    /// <param name="tokenColumns">토큰이 위치한 열 번호입니다.</param>
    /// <returns>생성된 토큰 객체를 반환합니다.</returns>
    private static Token MakeToken(TokenType tokenType, ReadOnlyMemory<char> rawText, string? text, long tokeLines,
        long tokenColumns)
    {
        text ??= tokenType switch
        {
            TokenType.StringLiteral or TokenType.CharacterLiteral =>
                // 앞뒤 문자(따옴표)제거
                rawText.ToString().Substring(1, rawText.Length - 2),
            TokenType.NumberLiteral =>
                // _기호 제거
                rawText.ToString().Replace("_", ""),
            _ => text
        };

        return new Token(
            tokenType,
            rawText,
            text,
            tokeLines,
            tokenColumns);
    }

    /// <summary>
    /// 규격 외의 공백 문자를 필터링하기 위한 정규식입니다.
    /// </summary>
    /// <returns>공백 문자 필터링에 사용될 정규식 객체입니다.</returns>
    [GeneratedRegex("^(?:\\s)+")]
    private static partial Regex ExtraWhitespaceFilterRegex();


    #region Enumerator

    /// <summary>
    /// 토크나이저의 열거자를 반환합니다.
    /// </summary>
    /// <returns>Token 열거자를 반환합니다.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// 토큰 열거자를 반환하는 메서드입니다.
    /// </summary>
    /// <returns>
    /// 토큰화된 스크립트를 열거할 수 있는 열거자를 반환합니다.
    /// </returns>
    IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 토크나이저의 열거자를 반환합니다.
    /// </summary>
    /// <returns>토크나이저의 열거자입니다.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 토크나이저에 대한 열거 작업을 지원하는 구조체입니다.
    /// </summary>
    public struct Enumerator(Tokenizer tokenizer) : IEnumerator<Token>
    {
        /// <summary>
        /// 토큰화 작업을 수행하는 Tokenizer 객체에 대한 참조입니다.
        /// </summary>
        private Tokenizer? _tokenizer = tokenizer;

        /// <summary>
        /// Enumerator가 사용 중인 리소스를 해제하고 관리되지 않는 리소스를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            _tokenizer = null;
        }

        /// <summary>
        /// 열거자를 다음 위치로 이동합니다.
        /// </summary>
        /// <returns>다음 위치로 성공적으로 이동하면 true를 반환하며, 그렇지 않으면 false를 반환합니다.</returns>
        /// <exception cref="ObjectDisposedException">열거자 또는 연결된 토크나이저가 이미 해제된 경우 발생합니다.</exception>
        public bool MoveNext()
        {
            ObjectDisposedException.ThrowIf(_tokenizer is null, nameof(Enumerator));
            return _tokenizer.NextToken() is not null;
        }

        /// <summary>
        /// 토크나이저 상태를 초기 상태로 재설정합니다.
        /// </summary>
        public void Reset()
        {
            ObjectDisposedException.ThrowIf(_tokenizer is null, nameof(Enumerator));
            _tokenizer.Reset();
        }

        /// <summary>
        /// 열거자의 현재 위치에서 반환되는 토큰입니다.
        /// </summary>
        public Token Current
        {
            get
            {
                ObjectDisposedException.ThrowIf(_tokenizer is null, nameof(Enumerator));
                return _tokenizer.CurrentToken.GetValueOrDefault();
            }
        }

        /// <summary>
        /// 현재 열거자가 가리키는 토큰입니다.
        /// </summary>
        object IEnumerator.Current => Current;
    }

    #endregion
}