using System.IO;
using DinoScript.Code;
using DinoScript.Parser;
using Xunit;
using Xunit.Abstractions;

namespace DinoScript.Test;

public partial class ParserTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ParserTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    private void CommonTest(string text, InternalCode[] expectedCodes, ParserMode parserMode,
        string? exceptionMessage = null)
    {
        try
        {
            var textReader = new StringReader(text);
            var parser = new SyntaxParser(textReader, new CodeGenerator(), parserMode);

            parser.Next();

            var codes = parser.CodeGenerator.Codes;
            testOutputHelper.WriteLine("Actual: ");
            for (var index = 0; index < codes.Count; index++)
            {
                var code = codes[index];
                testOutputHelper.WriteLine($"{index:0000}: {code.ToString()}");
            }

            testOutputHelper.WriteLine("");
            testOutputHelper.WriteLine("Expected: ");
            for (var index = 0; index < expectedCodes.Length; index++)
            {
                var code = expectedCodes[index];
                testOutputHelper.WriteLine($"{index:0000}: {code.ToString()}");
            }

            Assert.Equal(expectedCodes.Length, codes.Count);
            for (int i = 0; i < expectedCodes.Length; i++)
            {
                Assert.Equal(expectedCodes[i].Opcode, codes[i].Opcode);
                Assert.Equal(expectedCodes[i].Operands, codes[i].Operands);
            }
        }
        catch (SyntaxErrorException e) when (e.Message == exceptionMessage)
        {
            testOutputHelper.WriteLine("문법 오류 예외 체크");
            testOutputHelper.WriteLine(e.ToString());
        }
    }
}