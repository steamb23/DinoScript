using DinoScript.Code;
using DinoScript.Parser;

namespace DinoScript.Runtime;

public partial class VirtualMachine : IDisposable
{
    public IReadOnlyList<InternalCode> InternalCodes => Parser.CodeGenerator.Codes;
    private int internalCodeIndex = 0;

    public SyntaxParser Parser { get; private set; }

    public VirtualMachine(TextReader textReader, VirtualMachineOptions? options = null)
    {
        options ??= VirtualMachineOptions.Default;

        Memory = new VirtualMemory(options.StackSize);

        Parser = new SyntaxParser(textReader, options.ParserMode);
        textReader.ReadLine();
        internalCodeIndex = 0;
    }

    /// <summary>
    /// 다음 코드를 실행합니다.
    /// </summary>
    public void Next()
    {
        if (internalCodeIndex < InternalCodes.Count)
        {
            RunCode(InternalCodes[internalCodeIndex]);
            internalCodeIndex++;
        }
        // 내부 코드가 부족할 경우 추가 파싱 시도
        else
        {
            if (Parser.Next())
            {
                // EOT가 아닐 경우 재귀 호출
                Next();
            }
        }
    }

    /// <summary>
    /// 처음부터 끝까지 모든 코드를 실행합니다.
    /// </summary>
    public void Run()
    {
        while (internalCodeIndex < InternalCodes.Count || !Parser.IsEndOfText)
        {
            Next();
        }
    }

    public ResultView Result => new ResultView(Memory);

    public void Dispose()
    {
        Parser?.Dispose();
    }
}