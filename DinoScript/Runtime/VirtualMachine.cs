using System;
using System.Collections.Generic;
using System.IO;
using DinoScript.Code;
using DinoScript.Parser;

namespace DinoScript.Runtime
{
    public partial class VirtualMachine : IDisposable
    {
        public IReadOnlyList<InternalCode> InternalCodes => Parser.CodeGenerator.Codes;
        private int internalCodeIndex = 0;
        private VirtualMachineOptions options;

        public SyntaxParser Parser { get; private set; }

        public VirtualMachine(TextReader textReader, VirtualMachineOptions? options = null)
        {
            options ??= VirtualMachineOptions.Default;

            Parser = new SyntaxParser(textReader, new CodeGenerator(), options.ParserMode);
            textReader.ReadLine();

            this.options = options;

            Memory = new VirtualMemory(options.OperationStackSize, options.FunctionStackSize);
            internalCodeIndex = 0;
        }

        /// <summary>
        /// 메모리를 비우고 실행 상태를 리셋합니다.
        /// </summary>
        public void Reset()
        {
            Memory = new VirtualMemory(options.OperationStackSize, options.FunctionStackSize);
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

        public void Initialize()
        {
            while (!Parser.IsEndOfText)
            {
                Parser.Next();
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
}