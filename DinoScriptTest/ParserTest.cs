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
            "1+2*3+4",
            new[]
            {
                Make(Opcode.LoadConstant, 1),
                Make(Opcode.LoadConstant, 2),
                Make(Opcode.LoadConstant, 3),
                Make(Opcode.Multiply),
                Make(Opcode.Add),
                Make(Opcode.LoadConstant, 4),
                Make(Opcode.Add),
            }
        },
        new object[]
        {
            "1+2*(3+4)",
            new[]
            {
                Make(Opcode.LoadConstant, 1),
                Make(Opcode.LoadConstant, 2),
                Make(Opcode.LoadConstant, 3),
                Make(Opcode.LoadConstant, 4),
                Make(Opcode.Add),
                Make(Opcode.Multiply),
                Make(Opcode.Add),
            }
        },
        new object[]
        {
            "(1+2)*(3+4)",
            new[]
            {
                Make(Opcode.LoadConstant, 1),
                Make(Opcode.LoadConstant, 2),
                Make(Opcode.Add),
                Make(Opcode.LoadConstant, 3),
                Make(Opcode.LoadConstant, 4),
                Make(Opcode.Add),
                Make(Opcode.Multiply),
            }
        },
        new object[]
        {
            "(1+2)/(3+4)",
            new[]
            {
                Make(Opcode.LoadConstant, 1),
                Make(Opcode.LoadConstant, 2),
                Make(Opcode.Add),
                Make(Opcode.LoadConstant, 3),
                Make(Opcode.LoadConstant, 4),
                Make(Opcode.Add),
                Make(Opcode.Divide),
            }
        }
    };

    [Theory]
    [MemberData(nameof(ExpressionTestDataList))]
    public void ExpressionTest(string text, InternalCode[] expectedCodes)
    {
        var textReader = new StringReader(text);
        var parser = new SyntaxParser(textReader);

        parser.Next();

        var codes = parser.CodeGenerator.Codes;
        for (int i = 0; i < expectedCodes.Length; i++)
        {
            Assert.Equal(expectedCodes[i].Opcode, codes[i].Opcode);
            Assert.Equal(expectedCodes[i].Operands, codes[i].Operands);
            testOutputHelper.WriteLine(expectedCodes[i].ToString());
        }
    }
}