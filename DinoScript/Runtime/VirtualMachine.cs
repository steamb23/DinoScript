﻿using DinoScript.Code;
using DinoScript.Parser;

namespace DinoScript.Runtime;

public partial class VirtualMachine : IDisposable
{
    public IReadOnlyList<InternalCode>? InternalCodes => Parser?.CodeGenerator.Codes;
    private int internalCodeIndex = 0;

    public SyntaxParser? Parser { get; private set; }
    
    public VirtualMachine()
    {
    }

    public VirtualMachine(TextReader textReader, ParserMode parserMode = ParserMode.Full)
    {
        Initialize(textReader, parserMode);
    }

    public void Initialize(TextReader textReader, ParserMode parserMode = ParserMode.Full)
    {
        Parser = new SyntaxParser(textReader, parserMode);
        internalCodeIndex = 0;
    }

    /// <summary>
    /// 다음 코드를 실행합니다.
    /// </summary>
    public void Next()
    {
        if (Parser == null)
            return;

        if (internalCodeIndex < InternalCodes!.Count)
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

    public void Dispose()
    {
        Parser?.Dispose();
    }
}