using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Parser;
using DinoScript.Runtime;
using Xunit;

namespace DinoScript.Test;

using static InternalCode;

public class VirtualMachineTest
{
    public static IEnumerable<object[]> ExpressionTestDataList => new[]
    {
        new object[]
        {
            "1 + 2 * 3 + 4",
            11
        },
    };
    
    [Theory]
    [MemberData(nameof(ExpressionTestDataList))]
    public void ExpressionTest(string text, int expectedValue)
    {
        var vm = new VirtualMachine(new StringReader(text), new()
        {
            ParserMode = ParserMode.ExpressionTest,
        });
        
        vm.Run();
        
        // 결과값 가져오기
        var result = vm.Memory.Stack.PeekDouble();
        
        Assert.Equal(expectedValue, result);
    }
}