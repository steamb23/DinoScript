// using System;
// using System.Collections.Generic;
// using DinoScript.Parser;
// using DinoScript.Syntax;
//
// namespace DinoScript.Code
// {
//     public class CodeGeneratorLegacy
//     {
//         private List<InternalCode> codes = new List<InternalCode>();
//
//         private Stack<InternalCode> expressionStack = new Stack<InternalCode>();
//         private Queue<InternalCode> expressionQueue = new Queue<InternalCode>();
//
//         // 브랜치 명령의 코드 위치를 저장하는 목록입니다.
//         private List<int> expressionBranchIndices = new List<int>();
//
//         private bool needCodeCheck = false;
//
//         private InternalCode lastEnqueuedCode;
//
//         private void Enqueue(InternalCode code)
//         {
//             expressionQueue.Enqueue(code);
//             lastEnqueuedCode = code;
//         }
//
//         public IReadOnlyList<InternalCode> Codes => codes;
//
//         /// <summary>
//         /// 토큰의 값을 코드 큐에 넣습니다.
//         /// </summary>
//         /// <param name="token"></param>
//         public void AccessTokenEnqueue(Token token)
//         {
//             InternalCode code = token.Type switch
//             {
//                 TokenType.NumberLiteral => long.TryParse(token.Value!, out var longValue)
//                     ? InternalCode.Make(Opcode.LoadConstantInteger, token, longValue)
//                     : InternalCode.Make(Opcode.LoadConstantNumber, token, double.Parse(token.Value!)),
//                 TokenType.BooleanLiteral =>
//                     InternalCode.Make(Opcode.LoadConstantBoolean, token, (long)(bool.Parse(token.Value!) ? 1 : 0)),
//                 _ => InternalCode.Make(Opcode.NoOperation, token)
//             };
//
//             Enqueue(code);
//         }
//
//         public void UnaryTokenEnqueue(UnaryOperator unaryOperator, Token token)
//         {
//             var code = unaryOperator switch
//             {
//                 UnaryOperator.Minus => InternalCode.Make(Opcode.Negative, token),
//                 UnaryOperator.Plus => InternalCode.Make(Opcode.NoOperation, token),
//                 UnaryOperator.Not => InternalCode.Make(Opcode.NoOperation, token), // TODO: Equal 연산자 구현이 필요함
//                 _ => InternalCode.Make(Opcode.NoOperation, token)
//             };
//
//             Enqueue(code);
//         }
//
//         /// <summary>
//         /// 토큰을 연산자로 재분류하고 코드 스택에 넣습니다.
//         /// </summary>
//         /// <param name="binaryOperator"></param>
//         /// <param name="token"></param>
//         public void OperatorTokenPush(BinaryOperator binaryOperator, Token token)
//         {
//             switch (binaryOperator)
//             {
//                 // 산술 연산자
//                 case BinaryOperator.Add:
//                     OpcodePush(Opcode.Add, token);
//                     break;
//                 case BinaryOperator.Subtract:
//                     OpcodePush(Opcode.Subtract, token);
//                     break;
//                 case BinaryOperator.Multiply:
//                     OpcodePush(Opcode.Multiply, token);
//                     break;
//                 case BinaryOperator.Divide:
//                     OpcodePush(Opcode.Divide, token);
//                     break;
//                 case BinaryOperator.Modulo:
//                     OpcodePush(Opcode.Modulo, token);
//                     break;
//                 // 비교 연산자
//                 case BinaryOperator.Equal:
//                     OpcodePush(Opcode.Equal, token);
//                     break;
//                 case BinaryOperator.NotEqual:
//                     OpcodePush(Opcode.NotEqual, token);
//                     break;
//                 case BinaryOperator.GreaterThanOrEqual:
//                     OpcodePush(Opcode.GreaterThanOrEqual, token);
//                     break;
//                 case BinaryOperator.LessThanOrEqual:
//                     OpcodePush(Opcode.LessThanOrEqual, token);
//                     break;
//                 case BinaryOperator.GreaterThan:
//                     OpcodePush(Opcode.GreaterThan, token);
//                     break;
//                 case BinaryOperator.LessThan:
//                     OpcodePush(Opcode.LessThan, token);
//                     break;
//                 // 논리 연산자
//                 case BinaryOperator.And:
//                 {
//                     OpcodeEnqueue(Opcode.BranchIfFalse, token, -1);
//                     needCodeCheck = true;
//                     break;
//                 }
//                 case BinaryOperator.Or:
//                 {
//                     OpcodeEnqueue(Opcode.BranchIfTrue, token, -1);
//                     needCodeCheck = true;
//                     break;
//                 }
//                 default:
//                     OpcodePush(Opcode.NoOperation, token);
//                     break;
//             }
//         }
//
//         private void OpcodePush(Opcode opcode, Token token)
//         {
//             expressionStack.Push(InternalCode.Make(opcode, token));
//         }
//
//         private void OpcodePush(Opcode opcode, Token token, long operand)
//         {
//             expressionStack.Push(InternalCode.Make(opcode, token, operand));
//         }
//
//         private void OpcodeEnqueue(Opcode opcode, Token token, int operand)
//         {
//             expressionQueue.Enqueue(InternalCode.Make(opcode, token, operand));
//         }
//
//         /// <summary>
//         /// 표현식 코드의 축적을 마치고 코드 리스트에 출력합니다.
//         /// </summary>
//         public void GenerateExpression(BinaryOperator binaryOperator, Token? token = null)
//         {
//             var lastCodeIndex = Math.Max(0, codes.Count - 1);
//             var expressionQueueCount = expressionQueue.Count;
//             //var expressionStackCount = expressionStack.Count;
//
//             codes.AddRange(expressionQueue);
//             expressionQueue.Clear();
//             codes.AddRange(expressionStack);
//             expressionStack.Clear();
//
//             // 오퍼레이터에 대한 체크가 필요할 경우 오퍼레이터 시작 지점에서 부터 코드의 끝까지 코드를 체크합니다.
//             if (needCodeCheck)
//             {
//                 var operatorStartIndex = lastCodeIndex + expressionQueueCount;
//                 for (int i = lastCodeIndex; i < codes.Count; i++)
//                 {
//                     var code = codes[i];
//
//                     switch (code.Opcode)
//                     {
//                         case Opcode.Branch:
//                         case Opcode.BranchIfFalse:
//                         case Opcode.BranchIfTrue:
//                             expressionBranchIndices.Add(i);
//                             break;
//                     }
//                 }
//
//                 needCodeCheck = false;
//             }
//
//             // 이항 연산자에 대응되는 최종 코드 가공
//             switch (binaryOperator)
//             {
//                 case BinaryOperator.And:
//                 case BinaryOperator.Or:
//                 {
//                     if (expressionBranchIndices.Count > 0)
//                     {
//                         var failCodeIndex = codes.Count + 1;
//                         codes.Add(InternalCode.Make(Opcode.Branch, token!, failCodeIndex + 1));
//                         codes.Add(InternalCode.Make(Opcode.LoadConstantBoolean, token!,
//                             (long)(binaryOperator == BinaryOperator.Or ? 1 : 0)));
//                         codes.Add(InternalCode.Make(Opcode.NoOperation, null));
//
//                         foreach (var branchIndex in expressionBranchIndices)
//                         {
//                             var prevCode = codes[branchIndex];
//                             codes[branchIndex] = InternalCode.Make(prevCode.Opcode, prevCode.Token, failCodeIndex);
//                         }
//
//                         expressionBranchIndices.Clear();
//                     }
//
//                     break;
//                 }
//             }
//         }
//
//         private InternalCode FlipLoadAndStore(InternalCode code)
//         {
//             switch (code.Opcode)
//             {
//                 case Opcode.LoadFromLocal:
//                     return InternalCode.Make(code.Opcode, code.Token);
//                 default:
//                     return code;
//             }
//         }
//     }
// }