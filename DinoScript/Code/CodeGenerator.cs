using System.Collections.Generic;
using DinoScript.Parser;
using DinoScript.Syntax;

namespace DinoScript.Code
{
    public class CodeGenerator
    {
        private List<InternalCode> codes = new List<InternalCode>();

        private Stack<InternalCode> expressionStack = new Stack<InternalCode>();
        private Queue<InternalCode> expressionQueue = new Queue<InternalCode>();

        private InternalCode lastEnqueuedCode;

        private void Enqueue(InternalCode code)
        {
            expressionQueue.Enqueue(code);
            lastEnqueuedCode = code;
        }

        public IReadOnlyList<InternalCode> Codes => codes;

        /// <summary>
        /// 토큰의 값을 코드 큐에 넣습니다.
        /// </summary>
        /// <param name="token"></param>
        public void AccessTokenEnqueue(Token token)
        {
            InternalCode code = token.Type switch
            {
                TokenType.NumberLiteral => InternalCode.Make(Opcode.LoadConstantNumber, token,
                    double.Parse(token.Value!)),
                _ => InternalCode.Make(Opcode.NoOperation, token)
            };

            Enqueue(code);
        }

        public void UnaryTokenEnqueue(UnaryOperator unaryOperator, Token token)
        {
            var code = unaryOperator switch
            {
                UnaryOperator.Minus => InternalCode.Make(Opcode.Negative, token),
                UnaryOperator.Plus => InternalCode.Make(Opcode.NoOperation, token),
                UnaryOperator.Not => InternalCode.Make(Opcode.NoOperation, token), // TODO: Equal 연산자 구현이 필요함
                _ => InternalCode.Make(Opcode.NoOperation, token)
            };

            Enqueue(code);
        }

        /// <summary>
        /// 토큰을 연산자로 재분류하고 코드 스택에 넣습니다.
        /// </summary>
        /// <param name="binaryOperator"></param>
        /// <param name="token"></param>
        public void OperatorTokenPush(BinaryOperator binaryOperator, Token token)
        {
            var opcode = binaryOperator switch
            {
                BinaryOperator.Add => Opcode.Add,
                BinaryOperator.Subtract => Opcode.Subtract,
                BinaryOperator.Multiply => Opcode.Multiply,
                BinaryOperator.Divide => Opcode.Divide,
                BinaryOperator.Modulo => Opcode.Modulo,
                _ => Opcode.NoOperation
            };

            expressionStack.Push(InternalCode.Make(opcode, token));
        }

        /// <summary>
        /// 표현식 코드의 축적을 마치고 코드 리스트에 출력합니다.
        /// </summary>
        public void GenerateExpression()
        {
            codes.AddRange(expressionQueue);
            expressionQueue.Clear();
            codes.AddRange(expressionStack);
            expressionStack.Clear();
        }

        private InternalCode FlipLoadAndStore(InternalCode code)
        {
            switch (code.Opcode)
            {
                case Opcode.LoadFromLocal:
                    return InternalCode.Make(code.Opcode, code.Token);
                default:
                    return code;
            }
        }
    }
}