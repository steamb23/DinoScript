#nullable disable

using System.Collections.Generic;
using System.IO;
using DinoScript.Parser;
using DinoScript.Syntax;
using Xunit;

namespace DinoScript.Test;

public class TokenizerTest
{
    public class TokenTestData
    {
        public string Text { get; }
        public List<Token> Token { get; }
    }

    private static Token MakeTestToken(TokenType tokenType, string value)
    {
        return new Token(tokenType, value);
    }

    public static IEnumerable<object[]> TokenTestDataList => new[]
    {
        new object[]
        {
            "10*10",
            new[]
            {
                MakeTestToken(TokenType.NumberLiteral,
                    "10"),
                MakeTestToken(TokenType.Operator,
                    "*"),
                MakeTestToken(TokenType.NumberLiteral,
                    "10"),
            }
        },
        new object[]
        {
            "10 * 10",
            new[]
            {
                MakeTestToken(TokenType.NumberLiteral,
                    "10"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "*"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.NumberLiteral,
                    "10"),
            }
        },
        new object[]
        {
            "\"Hello, \" + \"World!\"",
            new[]
            {
                MakeTestToken(TokenType.StringLiteral,
                    "Hello, "),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "+"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.StringLiteral,
                    "World!"),
            }
        },
        new object[]
        {
            "( a + b ) * 10",
            new[]
            {
                MakeTestToken(TokenType.Mark,
                    "("),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Identifier,
                    "a"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "+"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Identifier,
                    "b"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Mark,
                    ")"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.Operator,
                    "*"),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.NumberLiteral,
                    "10")
            }
        },
        new object[]
        {
            "( \t)",
            new[]
            {
                MakeTestToken(TokenType.Mark,
                    "("),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.WhiteSpace,
                    "\t"),
                MakeTestToken(TokenType.Mark,
                    ")")
            }
        },
        new object[]
        {
            "test\\\r\na",
            new[]
            {
                MakeTestToken(TokenType.Identifier,
                    "test"),
                MakeTestToken(TokenType.WhiteSpace,
                    "\\\r\n"),
                MakeTestToken(TokenType.Identifier,
                    "a")
            }
        }
    };

    /// <summary>
    /// 토큰화가 예상대로 진행되는지 테스트합니다.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="tokens"></param>
    [Theory]
    [MemberData(nameof(TokenTestDataList))]
    public void TokenizeTest(string text, Token[] tokens)
    {
        var tokenizer = new Tokenizer(new StringReader(text));

        var length = tokens.Length;

        var token = tokenizer.Current();
        for (int i = 0; i < length; i++)
        {
            Assert.NotNull(token);
            Assert.Equal(token.Type, tokens[i].Type);
            Assert.Equal(token.Value, tokens[i].Value);
            token = tokenizer.Next();
        }
    }
}