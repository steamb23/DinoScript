using DinoScript.Parser;
using DinoScript.Syntax;
using Xunit.Abstractions;

namespace DinoScript.Test;

public class TokenizerTest(ITestOutputHelper testOutputHelper)
{
    public sealed class TokenizeCase : IXunitSerializable
    {
        // ReSharper disable once UnusedMember.Global
        public TokenizeCase()
        {
        }

        public TokenizeCase(string caseName, string inputText, Token[] expectedTokens)
        {
            CaseName = caseName;
            InputText = inputText;
            ExpectedTokens = expectedTokens;
        }

        public string? CaseName { get; private set; }
        public string? InputText { get; private set; }
        public Token[]? ExpectedTokens { get; private set; }

        public override string? ToString() => CaseName ?? "Unnamed Test Case";

        public void Deserialize(IXunitSerializationInfo info)
        {
            CaseName = info.GetValue<string>(nameof(CaseName));
            InputText = info.GetValue<string>(nameof(InputText));
            ExpectedTokens = info.GetValue<Token[]>(nameof(ExpectedTokens));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(CaseName), CaseName);
            info.AddValue(nameof(InputText), InputText);
            info.AddValue(nameof(ExpectedTokens), ExpectedTokens);
        }
    }

    private static Token MakeTestToken(TokenType tokenType, string text, string? redefinedText = null) =>
        new(tokenType, text.AsMemory(), redefinedText);

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