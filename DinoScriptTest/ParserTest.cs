using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Code.Generator;
using DinoScript.Parser;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

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
            "1 + 2 + 3 + 4",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)4),
                Make(Opcode.Add, null),
            }
        },
        new object[]
        {
            "1 + 2 * 3 + 4",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.Multiply, null),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)4),
                Make(Opcode.Add, null),
            }
        },
        new object[]
        {
            "1+2*(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.LoadConstantInteger, null, (long)4),
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
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.LoadConstantInteger, null, (long)4),
                Make(Opcode.Add, null),
                Make(Opcode.Multiply, null),
            }
        },
        new object[]
        {
            "(1+2)/(3+4)",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.LoadConstantInteger, null, (long)4),
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
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.LoadConstantInteger, null, (long)4),
                Make(Opcode.Add, null),
                Make(Opcode.Divide, null),
            }
        },
        new object[]
        {
            "(1+2)*(3+4)\n",
            new[]
            {
                Make(Opcode.LoadConstantInteger, null, (long)1),
                Make(Opcode.LoadConstantInteger, null, (long)2),
                Make(Opcode.Add, null),
                Make(Opcode.LoadConstantInteger, null, (long)3),
                Make(Opcode.LoadConstantInteger, null, (long)4),
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
                Make(Opcode.LoadConstantInteger, null, (long)-10),
            }
        },
        new object[]
        {
            "not true",
            new[]
            {
                Make(Opcode.LoadConstantBoolean, null, (long)1),
                Make(Opcode.LoadConstantBoolean, null, (long)0),
                Make(Opcode.Equal, null),
            }
        },
        new object[]
        {
            "10 > 5 and 30 > 15",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantInteger, null, (long)10),
                /*01*/Make(Opcode.LoadConstantInteger, null, (long)5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfFalse, null, 0x08),
                /*04*/Make(Opcode.LoadConstantInteger, null, (long)30),
                /*05*/Make(Opcode.LoadConstantInteger, null, (long)15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.Branch, null, 0x09),
                /*08*/Make(Opcode.LoadConstantBoolean, null, (long)0), // 실패 처리
            }
        },
        new object[]
        {
            "10 > 5 and 30 > 15 and 50 > 25",
            new[]
            {
                // and 체이닝
                /*00*/Make(Opcode.LoadConstantInteger, null, (long)10),
                /*01*/Make(Opcode.LoadConstantInteger, null, (long)5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfFalse, null, 0x0c),
                /*04*/Make(Opcode.LoadConstantInteger, null, (long)30),
                /*05*/Make(Opcode.LoadConstantInteger, null, (long)15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.BranchIfFalse, null, 0x0c),
                /*08*/Make(Opcode.LoadConstantInteger, null, (long)50),
                /*09*/Make(Opcode.LoadConstantInteger, null, (long)25),
                /*0A*/Make(Opcode.GreaterThan, null),
                /*0B*/Make(Opcode.Branch, null, 0x0d),
                /*0C*/Make(Opcode.LoadConstantBoolean, null, (long)0), // 실패 처리
            }
        },
        new object[]
        {
            "10 > 5 or 30 > 15",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantInteger, null, (long)10),
                /*01*/Make(Opcode.LoadConstantInteger, null, (long)5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfTrue, null, 0x08),
                /*04*/Make(Opcode.LoadConstantInteger, null, (long)30),
                /*05*/Make(Opcode.LoadConstantInteger, null, (long)15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.Branch, null, 0x09),
                /*08*/Make(Opcode.LoadConstantBoolean, null, (long)1), // 성공 처리
            }
        },
        new object[]
        {
            "10 > 5 or 30 > 15 or 50 > 25",
            new[]
            {
                // or 체이닝
                /*00*/Make(Opcode.LoadConstantInteger, null, (long)10),
                /*01*/Make(Opcode.LoadConstantInteger, null, (long)5),
                /*02*/Make(Opcode.GreaterThan, null),
                /*03*/Make(Opcode.BranchIfTrue, null, 0x0c),
                /*04*/Make(Opcode.LoadConstantInteger, null, (long)30),
                /*05*/Make(Opcode.LoadConstantInteger, null, (long)15),
                /*06*/Make(Opcode.GreaterThan, null),
                /*07*/Make(Opcode.BranchIfTrue, null, 0x0c),
                /*08*/Make(Opcode.LoadConstantInteger, null, (long)50),
                /*09*/Make(Opcode.LoadConstantInteger, null, (long)25),
                /*0A*/Make(Opcode.GreaterThan, null),
                /*0B*/Make(Opcode.Branch, null, 0x0d),
                /*0C*/Make(Opcode.LoadConstantBoolean, null, (long)1), // 성공 처리
            }
        },
        new object[]
        {
            "true and false or true",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantBoolean, null, (long)1),
                /*01*/Make(Opcode.BranchIfFalse, null, 0x04), // and
                /*02*/Make(Opcode.LoadConstantBoolean, null, (long)0),
                /*03*/Make(Opcode.BranchIfTrue, null, 0x06), // or
                /*04*/Make(Opcode.LoadConstantBoolean, null, (long)1), // and skip
                /*05*/Make(Opcode.Branch, null, 0x07),
                /*06*/Make(Opcode.LoadConstantBoolean, null, (long)1), // or skip
            }
        },
        new object[]
        {
            "true or false and true",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantBoolean, null, (long)1),
                /*01*/Make(Opcode.BranchIfTrue, null, 0x08), // or
                /*02*/Make(Opcode.LoadConstantBoolean, null, (long)0),
                /*03*/Make(Opcode.BranchIfFalse, null, 0x06), // and
                /*04*/Make(Opcode.LoadConstantBoolean, null, (long)1),
                /*05*/Make(Opcode.Branch, null, 0x07),
                /*06*/Make(Opcode.LoadConstantBoolean, null, (long)0), // and skip
                /*07*/Make(Opcode.Branch, null, 0x09),
                /*08*/Make(Opcode.LoadConstantBoolean, null, (long)1), // or skip
            }
        },
        new object[]
        {
            "false and true or false",
            new[]
            {
                /*00*/Make(Opcode.LoadConstantBoolean, null, (long)0),
                /*01*/Make(Opcode.BranchIfFalse, null, 0x04), // and
                /*02*/Make(Opcode.LoadConstantBoolean, null, (long)1),
                /*03*/Make(Opcode.BranchIfTrue, null, 0x06), // or
                /*04*/Make(Opcode.LoadConstantBoolean, null, (long)0), // and skip
                /*05*/Make(Opcode.Branch, null, 0x07),
                /*06*/Make(Opcode.LoadConstantBoolean, null, (long)1), // or skip
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
            var parser = new SyntaxParser(textReader, new CodeGenerator(), ParserMode.ExpressionTest);

            parser.Next();

            var codes = parser.CodeGenerator.Codes;
            testOutputHelper.WriteLine("Actual: ");
            foreach (var code in codes)
            {
                testOutputHelper.WriteLine(code.ToString());
            }

            testOutputHelper.WriteLine("");
            testOutputHelper.WriteLine("Expected: ");
            foreach (var code in expectedCodes)
            {
                testOutputHelper.WriteLine(code.ToString());
            }

            Assert.Equal(expectedCodes.Length, codes.Count);
            for (int i = 0; i < expectedCodes.Length; i++)
            {
                Assert.Equal(expectedCodes[i].Opcode, codes[i].Opcode);
                Assert.Equal(expectedCodes[i].Operands, codes[i].Operands);
            }
        }
        catch (SyntaxErrorException e)
        {
            testOutputHelper.WriteLine("문법 오류 예외 체크");
            testOutputHelper.WriteLine(e.ToString());
        }
    }
}