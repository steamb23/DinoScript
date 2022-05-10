﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public void UnaryTokenEnqueue(Token token)
        {
            var code = token.Value switch
            {
                "-" => InternalCode.Make(Opcode.Negative, token),
                "+" => InternalCode.Make(Opcode.NoOperation, token),
                "!" => InternalCode.Make(Opcode.NoOperation, token), // TODO: Equal 연산자 구현이 필요함
                _ => InternalCode.Make(Opcode.NoOperation, token)
            };

            Enqueue(code);
        }

        /// <summary>
        /// 토큰을 연산자로 재분류하고 코드 스택에 넣습니다.
        /// </summary>
        /// <param name="token"></param>
        public void OperatorTokenPush(Token token)
        {
            var opcode = token.Value switch
            {
                "+" => Opcode.Add,
                "-" => Opcode.Subtract,
                "*" => Opcode.Multiply,
                "/" => Opcode.Divide,
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