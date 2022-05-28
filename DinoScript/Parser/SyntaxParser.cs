using System;
using System.Collections.Generic;
using System.IO;
using DinoScript.Code;

namespace DinoScript.Parser
{
    public partial class SyntaxParser : IDisposable
    {
        private Tokenizer Tokenizer { get; }

        private List<InternalCode> codes = new List<InternalCode>();

        // public CodeGeneratorLegacy CodeGeneratorLegacy { get; } = new CodeGeneratorLegacy();
        
        public CodeGenerator CodeGenerator { get; }

        public ParserMode ParserMode { get; }

        public FunctionState RootFunctionState { get; } = new FunctionState(0);

        public FunctionState CurrentFunctionState { get; internal set; }

        public SyntaxParser(TextReader textReader, CodeGenerator codeGenerator, ParserMode parserMode = ParserMode.Full)
        {
            Tokenizer = new Tokenizer(textReader);
            CodeGenerator = codeGenerator;
            ParserMode = parserMode;
            
            CurrentFunctionState = RootFunctionState;
        }

        public bool IsEndOfText => Tokenizer.IsEndOfText;

        /// <summary>
        /// 토큰을 읽어 다음 내부 코드를 생성합니다.
        /// </summary>
        /// <returns>토크나이저의 텍스트가 끝에 도달하면 false입니다.</returns>
        public bool Next()
        {
            if (Tokenizer.IsEndOfText)
                return false;
            Root();
            return true;
        }

        private void Root()
        {
            switch (ParserMode)
            {
                case ParserMode.ExpressionTest:
                {
                    ExpressionDescription expressionDescription = ExpressionDescription.Empty;
                    Expression(ref expressionDescription);
                    break;
                }
                case ParserMode.Full:
                    throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            Tokenizer.Dispose();
        }
    }
}