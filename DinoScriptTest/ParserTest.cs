using System;
using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Parser;
using Xunit;
using Xunit.Abstractions;

namespace DinoScript.Test;

using static InternalCode;

public class ParserTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ParserTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    public static IEnumerable<object[]> ExpressionTestDataList => new[]
    {
        new object[]
        {
            "1 + 2 * 3 + 4",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 1),
                Make(Opcode.LoadConstantNumber, null, 2),
                Make(Opcode.LoadConstantNumber, null, 3),
                Make(Opcode.Multiply, null),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantNumber, null, 4),
                Make(Opcode.Add, null),
            }
        },
        new object[]
        {
            "1+2*(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 1),
                Make(Opcode.LoadConstantNumber, null, 2),
                Make(Opcode.LoadConstantNumber, null, 3),
                Make(Opcode.LoadConstantNumber, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Multiply, null),
                Make(Opcode.Add, null),
            }
        },
        new object[]
        {
            "(1+2)*(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 1),
                Make(Opcode.LoadConstantNumber, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantNumber, null, 3),
                Make(Opcode.LoadConstantNumber, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Multiply, null),
            }
        },
        new object[]
        {
            "(1+2)/(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 1),
                Make(Opcode.LoadConstantNumber, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantNumber, null, 3),
                Make(Opcode.LoadConstantNumber, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Divide, null),
            }
        },
        // 예외
        new object[]
        {
            "(1+2)*(3@4)",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 1),
                Make(Opcode.LoadConstantNumber, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantNumber, null, 3),
                Make(Opcode.LoadConstantNumber, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Divide, null),
            }
        },
        new object[]
        {
            "(1+2)*(3+4)\n",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 1),
                Make(Opcode.LoadConstantNumber, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantNumber, null, 3),
                Make(Opcode.LoadConstantNumber, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Multiply, null),
            }
        }
    };

    [Theory]
    [MemberData(nameof(ExpressionTestDataList))]
    public void ExpressionTest(string text, InternalCode[] expectedCodes)
    {
        try
        {
            var textReader = new StringReader(text);
            var parser = new SyntaxParser(textReader, ParserMode.ExpressionTest);

            parser.Next();

            var codes = parser.CodeGenerator.Codes;
            for (int i = 0; i < expectedCodes.Length; i++)
            {
                Assert.Equal(expectedCodes[i].Opcode, codes[i].Opcode);
                Assert.Equal(expectedCodes[i].Operands, codes[i].Operands);
                testOutputHelper.WriteLine(expectedCodes[i].ToString());
            }
        }
        catch (SyntaxErrorException e)
        {
            testOutputHelper.WriteLine("문법 오류 예외 체크");
            testOutputHelper.WriteLine(e.ToString());
        }
    }
}