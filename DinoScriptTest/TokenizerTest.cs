#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using DinoScript.Parser;
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
        return new Token()
        {
            Type = tokenType,
            Value = value
        };
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
                MakeTestToken(TokenType.Punctuator,
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
                MakeTestToken(TokenType.Punctuator,
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
                MakeTestToken(TokenType.Punctuator,
                    "("),
                MakeTestToken(TokenType.WhiteSpace,
                    " "),
                MakeTestToken(TokenType.WhiteSpace,
                    "\t"),
                MakeTestToken(TokenType.Punctuator,
                    ")")
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

        for (int i = 0; i < length; i++)
        {
            var token = tokenizer.Next();
            Assert.NotNull(token);
            Assert.Equal(token.Type, tokens[i].Type);
            Assert.Equal(token.Value, tokens[i].Value);
        }
    }
}