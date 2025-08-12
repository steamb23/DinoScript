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

using DinoScript.Parser;
using DinoScript.Parser.Syntax;
using Xunit.Abstractions;

namespace DinoScript.Test;

/// <summary>
/// TokenizerTest 클래스는 텍스트를 토큰화하는 로직을 테스트하기 위한 단위 테스트를 포함하고 있습니다.
/// </summary>
/// <remarks>
/// 이 클래스는 주어진 입력 텍스트와 예상되는 토큰 간의 일치 여부를 확인하기 위해
/// 다양한 테스트 케이스를 정의하고 검증합니다. 테스트는 XUnit 프레임워크와 함께 동작하며,
/// <c>DataSource</c>를 통해 주입된 데이터를 기반으로 수행됩니다.
/// </remarks>
/// <example>
/// TokenizerTest 클래스는 숫자 리터럴, 연산자, 문자열 리터럴, 공백과 같은
/// 다양한 토큰의 처리를 확인하는 테스트 케이스를 제공합니다.
/// </example>
/// <seealso cref="IXunitSerializable"/>
/// <seealso cref="TheoryAttribute"/>
public class TokenizerTest(ITestOutputHelper testOutputHelper)
{
    /// <summary>
    /// TokenizeCase 클래스는 Xunit의 테스트 케이스 데이터를 나타내기 위해 구현되었습니다.
    /// 이 클래스는 테스트 데이터를 직렬화 및 역직렬화하여 Xunit의 테스트 프레임워크와 함께 사용될 수 있습니다.
    /// </summary>
    public sealed class TokenizeCase : IXunitSerializable
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// TokenizeCase 클래스는 개별 테스트 케이스의 정보를 보관하고 관리하는 데이터 구조입니다.
        /// 이 클래스는 IXunitSerializable 인터페이스를 구현하여 xUnit 테스트 프레임워크에서의
        /// 직렬화 및 역직렬화를 지원합니다.
        /// </summary>
        public TokenizeCase()
        {
        }

        /// <summary>
        /// 특정 테스트 사례를 정의하기 위한 클래스입니다.
        /// 테스트 이름, 입력 텍스트, 그리고 예상 토큰 목록을 저장합니다.
        /// </summary>
        public TokenizeCase(string caseName, string inputText, Token[] expectedTokens)
        {
            CaseName = caseName;
            InputText = inputText;
            ExpectedTokens = expectedTokens;
        }

        /// 이 속성은 테스트 케이스의 이름을 나타냅니다.
        /// 테스트 이름은 각 테스트 케이스를 식별하기 위한 문자열 값입니다.
        /// 이 속성은 읽기 전용(private set)으로 설정되어 있으며, 생성자 또는
        /// IXunitSerializable 인터페이스의 Deserialize 메서드로 값을 할당할 수 있습니다.
        public string? CaseName { get; private set; }

        /// <summary>
        /// 입력된 텍스트를 나타내는 속성입니다.
        /// </summary>
        /// <remarks>
        /// 테스트 케이스에서 토크나이즈 처리에 사용되는 입력 텍스트를 보관합니다.
        /// 해당 속성은 private set 접근자를 가지며, 이를 통해 외부에서 값을 수정하지 못하도록 합니다.
        /// </remarks>
        public string? InputText { get; private set; }

        /// <summary>
        /// 테스트 케이스에서 기대되는 토큰 배열을 나타냅니다.
        /// 이 속성은 입력 텍스트를 토큰화 과정 후 얻어지는 결과와 비교하기 위해 사용됩니다.
        /// </summary>
        public Token[]? ExpectedTokens { get; private set; }

        /// 이 메서드는 객체를 나타내는 문자열 표현을 반환합니다.
        /// <returns>
        /// 테스트 케이스의 이름(CaseName)을 문자열로 반환합니다.
        /// 만약 이름이 설정되지 않은 경우 "Unnamed Test Case"를 반환합니다.
        /// </returns>
        public override string? ToString() => CaseName ?? "Unnamed Test Case";

        /// <summary>
        /// IXunitSerializationInfo에서 정보를 역직렬화하여 객체의 상태를 설정합니다.
        /// </summary>
        /// <param name="info">역직렬화에 사용되는 IXunitSerializationInfo 정보입니다.</param>
        public void Deserialize(IXunitSerializationInfo info)
        {
            CaseName = info.GetValue<string>(nameof(CaseName));
            InputText = info.GetValue<string>(nameof(InputText));
            ExpectedTokens = info.GetValue<Token[]>(nameof(ExpectedTokens));
        }

        /// <summary>
        /// IXunitSerializationInfo 객체에 데이터를 직렬화하는 메서드입니다.
        /// </summary>
        /// <param name="info">직렬화된 데이터를 저장할 IXunitSerializationInfo 객체입니다.</param>
        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(CaseName), CaseName);
            info.AddValue(nameof(InputText), InputText);
            info.AddValue(nameof(ExpectedTokens), ExpectedTokens);
        }
    }

    /// <summary>
    /// 지정된 TokenType과 텍스트를 기반으로 테스트용 토큰을 생성합니다.
    /// </summary>
    /// <param name="tokenType">생성할 토큰의 타입을 나타내는 TokenType 열거형 값입니다.</param>
    /// <param name="text">생성할 토큰의 원본 텍스트입니다.</param>
    /// <param name="redefinedText">원본을 재정의할 텍스트입니다. 선택적으로 null일 수 있습니다.</param>
    /// <returns>생성된 Token 객체를 반환합니다.</returns>
    private static Token MakeTestToken(TokenType tokenType, string text, string? redefinedText = null) =>
        new(tokenType, text.AsMemory(), redefinedText);

    /// <summary>
    /// 데이터 소스에 대한 읽기 전용 프로퍼티입니다.
    /// 이 프로퍼티는 Tokenize 테스트 케이스 데이터를 제공하며,
    /// 다양한 입력 텍스트와 예상 토큰의 조합으로 구성되어 있습니다.
    /// 주로 테스트 메서드에서 이 데이터 소스를 기반으로 테스트 케이스를 실행합니다.
    /// </summary>
    public static TheoryData<TokenizeCase> DataSource { get; } =
    [
        new("곱셈 공백없음",
            "10*10",
            [
                MakeTestToken(TokenType.NumberLiteral,
                    "10"),
                MakeTestToken(TokenType.Operator,
                    "*"),
                MakeTestToken(TokenType.NumberLiteral,
                    "10")
            ]),
        new("곱셈 공백있음",
            "10 * 10",
            [
                MakeTestToken(TokenType.NumberLiteral,
                    "10"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "*"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.NumberLiteral,
                    "10")
            ]),
        new("문자열 조합",
            """
            "Hello, " + "World!"
            """,
            [
                MakeTestToken(TokenType.StringLiteral,
                    "\"Hello, \"", "Hello, "),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "+"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.StringLiteral,
                    "\"World!\"", "World!")
            ]),
        new("멀티라인 문자열",
            """
            "Hello,
              World!"
            """,
            [
                MakeTestToken(TokenType.StringLiteral,
                    "\"Hello,\r\n  World!\"",
                    "Hello,\r\nWorld!")
            ]),
        new("복합 수식",
            "( a + b ) * 10",
            [
                MakeTestToken(TokenType.Mark,
                    "("),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Identifier,
                    "a"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "+"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Identifier,
                    "b"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Mark,
                    ")"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "*"),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.NumberLiteral,
                    "10")
            ]),
        new("공백 문자 및 괄호",
            "( \t)",
            [
                MakeTestToken(TokenType.Mark,
                    "("),
                MakeTestToken(TokenType.Whitespace,
                    " "),
                MakeTestToken(TokenType.Whitespace,
                    "\t"),
                MakeTestToken(TokenType.Mark,
                    ")")
            ]),
        new("줄 넘김",
            """
            test\
            a
            """,
            [
                MakeTestToken(TokenType.Identifier,
                    "test"),
                MakeTestToken(TokenType.Whitespace,
                    "\\\r\n"),
                MakeTestToken(TokenType.Identifier,
                    "a")
            ])
    ];

    /// <summary>
    /// 입력 텍스트를 토큰화하고 결과가 예상된 토큰과 일치하는지 검증하는 테스트 메서드입니다.
    /// </summary>
    /// <param name="tokenizeCase">입력 텍스트와 예상된 토큰 정보를 포함한 테스트 케이스입니다.</param>
    [Theory(DisplayName = "Tokenizer: 기본 토큰화 케이스")]
    [MemberData(nameof(DataSource))]
    public void Tokenize_Should_Match_Expected(TokenizeCase tokenizeCase)
    {
        var tokenizer = new Tokenizer(tokenizeCase.InputText);

        testOutputHelper.WriteLine($"InputText:\n{tokenizeCase.InputText}");

        using var tokenizerEnumerator = tokenizer.GetEnumerator();

        Assert.NotNull(tokenizeCase.ExpectedTokens);
        foreach (var expectedToken in tokenizeCase.ExpectedTokens)
        {
            Assert.True(tokenizerEnumerator.MoveNext());
            var actualToken = tokenizerEnumerator.Current;
            testOutputHelper.WriteLine($"""
                                        ExpectedToken: {expectedToken}
                                        ActualToken: {actualToken}
                                        """);
            Assert.Equal(expectedToken.Type, actualToken.Type);
            Assert.Equal(expectedToken.RawText, actualToken.RawText);
            Assert.Equal(expectedToken.Value, actualToken.Value);
        }

        // 토큰 갯수 일치 체크
        Assert.False(tokenizerEnumerator.MoveNext());
    }
}