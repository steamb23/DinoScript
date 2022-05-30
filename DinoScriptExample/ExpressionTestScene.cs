using System.Diagnostics;
using TerraText;
using DinoScript;
using DinoScript.Parser;
using DinoScript.Runtime;

namespace DinoScriptExample;

public class ExpressionTestScene : Scene
{
    public override void Execute()
    {
        Console.Clear();
        Console.WriteLine("Expression Test...");
        Console.WriteLine();
        while (true)
        {
            Console.WriteLine("식을 입력하세요. (나가려면 exit 입력)");
            Console.Write('>');
            var input = ConsoleEx.ReadLineWithCursor();

            switch (input)
            {
                case "exit":
                    return;
                case "":
                    continue;
            }

            Stopwatch stopwatch = new Stopwatch();
            try
            {
                var machine = Script.Init(input, new VirtualMachineOptions(
                    ParserMode.ExpressionTest)
                );
                stopwatch.Restart();
                machine.Run();
                stopwatch.Stop();
                var result = machine.Result;
                switch (result.Value.Type)
                {
                    case DinoType.Integer:
                        Console.WriteLine($"결과: ({result.Value.Type}) {(long)machine.Result.Value}");
                        break;
                    case DinoType.Number:
                        Console.WriteLine($"결과: ({result.Value.Type}) {(double)machine.Result.Value}");
                        break;
                    case DinoType.Boolean:
                        Console.WriteLine($"결과: ({result.Value.Type}) {(bool)machine.Result.Value}");
                        break;
                }
                Console.WriteLine($"실행 시간: {stopwatch.Elapsed.TotalMilliseconds} ms");
                Console.WriteLine("생성된 코드:");
                foreach (var code in machine.InternalCodes)
                {
                    Console.WriteLine(code);
                }
                Console.WriteLine("-----------");
                machine.Dispose();
            }
            catch (Exception e)
            {
                // 예외 표시
                Console.WriteLine("실행중 예외가 발생했습니다:");
                Console.WriteLine(e);
            }
        }
    }
}