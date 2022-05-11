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
                Make(Opcode.LoadConstantInteger, null, 1),
                Make(Opcode.LoadConstantInteger, null, 2),
                Make(Opcode.LoadConstantInteger, null, 3),
                Make(Opcode.Multiply, null),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, 4),
                Make(Opcode.Add, null),
            }
        },
        new object[]
        {
            "1+2*(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, 1),
                Make(Opcode.LoadConstantInteger, null, 2),
                Make(Opcode.LoadConstantInteger, null, 3),
                Make(Opcode.LoadConstantInteger, null, 4),
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
                Make(Opcode.LoadConstantInteger, null, 1),
                Make(Opcode.LoadConstantInteger, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, 3),
                Make(Opcode.LoadConstantInteger, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Multiply, null),
            }
        },
        new object[]
        {
            "(1+2)/(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, 1),
                Make(Opcode.LoadConstantInteger, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, 3),
                Make(Opcode.LoadConstantInteger, null, 4),
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
                Make(Opcode.LoadConstantInteger, null, 1),
                Make(Opcode.LoadConstantInteger, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, 3),
                Make(Opcode.LoadConstantInteger, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Divide, null),
            }
        },
        new object[]
        {
            "(1+2)*(3+4)\n",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, 1),
                Make(Opcode.LoadConstantInteger, null, 2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, 3),
                Make(Opcode.LoadConstantInteger, null, 4),
                Make(Opcode.Add, null),
                Make(Opcode.Multiply, null),
            }
        },
        new object[]
        {
            "100_100.100_100",
            new[]
            {
                Make(Opcode.LoadConstantNumber, null, 100_100.100_100),
            }
        },
        new object[]
        {
            "-10",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, 10),
                Make(Opcode.Negative, null)
            }
        },
        new object[]
        {
            "not true",
            new[]
            {
                Make(Opcode.LoadConstantBoolean, null, 1),
                Make(Opcode.LoadConstantBoolean, null, 0),
                Make(Opcode.Equal, null),
            }
        },
        new object[]
        {
            "10 > 5 and 30 > 15",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantInteger, null, 10),
                /*01*/Make(Opcode.LoadConstantInteger, null, 5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfFalse, null, 0x08),
                /*04*/Make(Opcode.LoadConstantInteger, null, 30),
                /*05*/Make(Opcode.LoadConstantInteger, null, 15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.Branch, null, 0x09),
                /*08*/Make(Opcode.LoadConstantBoolean, null, 0), // 실패 처리
                /*09*/Make(Opcode.NoOperation, null, 0),
            }
        },
        new object[]
        {
            "10 > 5 and 30 > 15 and 50 > 25",
            new[]
            {
                // and 체이닝
                /*00*/Make(Opcode.LoadConstantInteger, null, 10),
                /*01*/Make(Opcode.LoadConstantInteger, null, 5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfFalse, null, 0x0c),
                /*04*/Make(Opcode.LoadConstantInteger, null, 30),
                /*05*/Make(Opcode.LoadConstantInteger, null, 15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.BranchIfFalse, null, 0x0c),
                /*08*/Make(Opcode.LoadConstantInteger, null, 30),
                /*09*/Make(Opcode.LoadConstantInteger, null, 15),
                /*0A*/Make(Opcode.GreaterThan, null),
                /*0B*/Make(Opcode.Branch, null, 0x0d),
                /*0C*/Make(Opcode.LoadConstantBoolean, null, 0), // 실패 처리
                /*0D*/Make(Opcode.NoOperation, null, 0),
            }
        },
        new object[]
        {
            "10 > 5 or 30 > 15",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantInteger, null, 10),
                /*01*/Make(Opcode.LoadConstantInteger, null, 5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfTrue, null, 0x08),
                /*04*/Make(Opcode.LoadConstantInteger, null, 30),
                /*05*/Make(Opcode.LoadConstantInteger, null, 15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.Branch, null, 0x09),
                /*08*/Make(Opcode.LoadConstantBoolean, null, 1), // 성공 처리
                /*09*/Make(Opcode.NoOperation, null, 0),
            }
        },
        new object[]
        {
            "10 > 5 or 30 > 15 or 50 > 25",
            new[]
            {
                // or 체이닝
                /*00*/Make(Opcode.LoadConstantInteger, null, 10),
                /*01*/Make(Opcode.LoadConstantInteger, null, 5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfTrue, null, 0x0c),
                /*04*/Make(Opcode.LoadConstantInteger, null, 30),
                /*05*/Make(Opcode.LoadConstantInteger, null, 15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.BranchIfTrue, null, 0x0c),
                /*08*/Make(Opcode.LoadConstantInteger, null, 30),
                /*09*/Make(Opcode.LoadConstantInteger, null, 15),
                /*0A*/Make(Opcode.GreaterThan, null),
                /*0B*/Make(Opcode.Branch, null, 0x0d),
                /*0C*/Make(Opcode.LoadConstantBoolean, null, 1), // 성공 처리
                /*0D*/Make(Opcode.NoOperation, null, 0),
            }
        },
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
            Assert.Equal(codes.Count, expectedCodes.Length);
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