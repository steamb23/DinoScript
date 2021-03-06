using System;
using DinoScript.Parser;
using DinoScript.Runtime;
using DinoScript.Syntax;

namespace DinoScript.Code
{
    public partial class CodeGenerator
    {
        internal void PatchBranchToHere(int branchCodeIndex)
        {
            PatchBranch(branchCodeIndex, Codes.Count);
        }
        
        /// <summary>
        /// 분기 코드 및 분기 연결 목록의 목적 인덱스를 변경합니다.
        /// </summary>
        /// <param name="branchCodeIndex"></param>
        /// <param name="targetCodeIndex"></param>
        /// <returns></returns>
        internal void PatchBranch(int branchCodeIndex, int targetCodeIndex)
        {
            if (branchCodeIndex == NoJump || targetCodeIndex == NoJump)
            {
                return;
            }

            var next = branchCodeIndex;
            do
            {
                var current = Math.Min(next, Codes.Count - 1);
                var code = Codes[current];
                switch (code.Opcode)
                {
                    case Opcode.Branch:
                    case Opcode.BranchIfFalse:
                    case Opcode.BranchIfTrue:
                        next = (int)code.Operands[0];
                        Codes[current] = InternalCode.Make(code.Opcode, code.Token, targetCodeIndex);
                        break;
                    default:
                        return;
                }
            } while (next != NoJump);
        }

        internal int GetBranchTargetIndex(int codeIndex)
        {
            var code = Codes[codeIndex];
            switch (code.Opcode)
            {
                case Opcode.Branch:
                case Opcode.BranchIfFalse:
                case Opcode.BranchIfTrue:
                    return (int)code.Operands[0];
                default:
                    return NoJump;
            }
        }

        /// <summary>
        /// 분기 코드의 목적 인덱스를 수정합니다.
        /// </summary>
        /// <param name="codeIndex"></param>
        /// <param name="targetCodeIndex"></param>
        internal void FixBranch(int codeIndex, int targetCodeIndex)
        {
            var code = Codes[codeIndex];
            switch (code.Opcode)
            {
                case Opcode.Branch:
                case Opcode.BranchIfFalse:
                case Opcode.BranchIfTrue:
                    Codes[codeIndex] = InternalCode.Make(code.Opcode, code.Token, targetCodeIndex);
                    break;
                default:
                    break;
            }
        }

        internal void FixBranchToHere(int codeIndex)
        {
            FixBranch(codeIndex, Codes.Count);
        }

        /// <summary>
        /// 분기 코드 및 분기 연결 목록을 가리키는 <see cref="descCodeIndex"/>를 <see cref="branchCodeIndex"/>가 가리키는 분기 목록에 합칩니다.
        /// </summary>
        /// <param name="branchCodeIndex"></param>
        /// <param name="descCodeIndex"></param>
        internal void BranchLinkConcat(ref int branchCodeIndex, int descCodeIndex)
        {
            if (descCodeIndex == NoJump)
                return;
            else if (branchCodeIndex == NoJump)
                branchCodeIndex = descCodeIndex;
            else
            {
                var codeIndex = branchCodeIndex;
                int next;

                // 마지막 분기 
                while ((next = GetBranchTargetIndex(codeIndex)) != NoJump)
                {
                    codeIndex = next;
                }

                FixBranch(codeIndex, descCodeIndex);
            }
        }

        public ExpressionDescription ExpressionInitialize(ExpressionKind kind, DinoValue value,
            in Token token, bool stackPush)
        {
            var codeIndex = Codes.Count;

            var exprDesc = ExpressionDescription.Empty;
            exprDesc.Kind = kind;
            exprDesc.Value = value;
            exprDesc.ValueCodeIndex = codeIndex;

            if (stackPush)
            {
                switch (exprDesc.Kind)
                {
                    case ExpressionKind.ConstantNumber:
                        Codes.Add(InternalCode.Make(Opcode.LoadConstantNumber, token, (double)exprDesc.Value));
                        break;
                    case ExpressionKind.ConstantInteger:
                        Codes.Add(InternalCode.Make(Opcode.LoadConstantInteger, token, (long)exprDesc.Value));
                        break;
                    case ExpressionKind.ConstantBoolean:
                        Codes.Add(InternalCode.Make(Opcode.LoadConstantBoolean, token, (long)exprDesc.Value));
                        break;
                    case ExpressionKind.LocalVariable:
                        Codes.Add(InternalCode.Make(Opcode.LoadFromLocal, token, (int)exprDesc.Value));
                        break;
                }
            }

            return exprDesc;
        }

        public void ExpressionEnd(ref ExpressionDescription exprDesc)
        {
            // And 연산자가 닫히지 않았을 경우
            if (exprDesc.BranchFalseCodeIndex != NoJump)
            {
                var codeIndex = Codes.Count;
                var lastAndCode = Codes[exprDesc.BranchFalseCodeIndex];
                Codes.Add(InternalCode.Make(Opcode.Branch, lastAndCode.Token, codeIndex + 2));
                PatchBranch(exprDesc.BranchFalseCodeIndex, codeIndex + 1);
                exprDesc.BranchFalseCodeIndex = NoJump;
                Codes.Add(InternalCode.Make(Opcode.LoadConstantBoolean, lastAndCode.Token, (long)0));
            }

            if (exprDesc.BranchTrueCodeIndex != NoJump)
            {
                var codeIndex = Codes.Count;
                var lastAndCode = Codes[exprDesc.BranchTrueCodeIndex];
                Codes.Add(InternalCode.Make(Opcode.Branch, lastAndCode.Token, codeIndex + 2));
                PatchBranch(exprDesc.BranchTrueCodeIndex, codeIndex + 1);
                exprDesc.BranchTrueCodeIndex = NoJump;
                Codes.Add(InternalCode.Make(Opcode.LoadConstantBoolean, lastAndCode.Token, (long)1));
            }
        }

        public void ExpressionPreProcessing(
            UnaryOperator unaryOperator,
            ref ExpressionDescription exprDesc,
            in Token token)
        {
            var targetCode = exprDesc.ValueCodeIndex >= 0 ? Codes[exprDesc.ValueCodeIndex] : new InternalCode();

            switch (unaryOperator)
            {
                case UnaryOperator.Minus:
                    switch (exprDesc.Kind)
                    {
                        case ExpressionKind.ConstantInteger:
                            var longValue = -(long)exprDesc.Value;
                            exprDesc.Value = longValue;
                            Codes[exprDesc.ValueCodeIndex] =
                                InternalCode.Make(Opcode.LoadConstantInteger, targetCode.Token, longValue);
                            break;
                        case ExpressionKind.ConstantNumber:
                            var doubleValue = -(double)exprDesc.Value;
                            exprDesc.Value = doubleValue;
                            Codes[exprDesc.ValueCodeIndex] =
                                InternalCode.Make(Opcode.LoadConstantNumber, targetCode.Token, doubleValue);
                            break;
                        case ExpressionKind.ConstantBoolean:
                            throw new SyntaxErrorException(token,
                                $"A constant of type boolean cannot be prefixed with a '{token.Text}' symbol.");
                        case ExpressionKind.LocalVariable:
                        case ExpressionKind.FunctionCall:
                            //TODO 변수 타입 체크 기능 추가 필요
                            Codes.Add(InternalCode.Make(Opcode.Negative, token));
                            break;
                        default:
                            throw new SyntaxErrorException(token);
                    }

                    break;
                case UnaryOperator.Not:
                    switch (exprDesc.Kind)
                    {
                        case ExpressionKind.ConstantBoolean:
                            var boolValue = !(bool)exprDesc.Value;
                            Codes[exprDesc.ValueCodeIndex] =
                                InternalCode.Make(Opcode.LoadConstantBoolean, targetCode.Token,
                                    (long)(boolValue ? 1 : 0));
                            break;
                        case ExpressionKind.ConstantInteger:
                        case ExpressionKind.ConstantNumber:
                            throw new SyntaxErrorException(token,
                                $"A constant of type number cannot be prefixed with a '{token.Text}' symbol.");
                        case ExpressionKind.LocalVariable:
                        case ExpressionKind.FunctionCall:
                            //TODO 변수 타입 체크 기능 추가 필요
                            Codes.Add(InternalCode.Make(Opcode.LoadConstantBoolean, token, (long)0));
                            Codes.Add(InternalCode.Make(Opcode.Equal, token));
                            break;
                        default:
                            throw new SyntaxErrorException(token);
                    }

                    break;
                case UnaryOperator.Plus:
                    // 아무것도 하지 않음
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unaryOperator), unaryOperator, null);
            }
        }

        public void ExpressionInterProcessing(
            BinaryOperator binaryOperator,
            ref ExpressionDescription exprDesc,
            in Token token)
        {
            int codeIndex;
            switch (binaryOperator)
            {
                case BinaryOperator.And:
                    codeIndex = Codes.Count;
                    Codes.Add(InternalCode.Make(Opcode.BranchIfFalse, token, NoJump));
                    BranchLinkConcat(ref exprDesc.BranchFalseCodeIndex, codeIndex);
                    break;
                case BinaryOperator.Or:
                    codeIndex = Codes.Count;
                    Codes.Add(InternalCode.Make(Opcode.BranchIfTrue, token, NoJump));
                    BranchLinkConcat(ref exprDesc.BranchTrueCodeIndex, codeIndex);
                    // and 연산자의 분기 위치를 이 명령의 다음 위치로 수정
                    if (exprDesc.BranchFalseCodeIndex != NoJump)
                    {
                        PatchBranch(exprDesc.BranchFalseCodeIndex, codeIndex + 1);
                        exprDesc.BranchFalseCodeIndex = NoJump;
                    }

                    break;
                default:
                    break;
            }
        }

        public void ExpressionPostProcessing(
            BinaryOperator binaryOperator,
            ref ExpressionDescription exprDesc,
            ref ExpressionDescription subExprDesc,
            in Token token)
        {
            switch (binaryOperator)
            {
                case BinaryOperator.Add:
                    Codes.Add(InternalCode.Make(Opcode.Add, token));
                    break;
                case BinaryOperator.Subtract:
                    Codes.Add(InternalCode.Make(Opcode.Subtract, token));
                    break;
                case BinaryOperator.Multiply:
                    Codes.Add(InternalCode.Make(Opcode.Multiply, token));
                    break;
                case BinaryOperator.Divide:
                    Codes.Add(InternalCode.Make(Opcode.Divide, token));
                    break;
                case BinaryOperator.Modulo:
                    Codes.Add(InternalCode.Make(Opcode.Modulo, token));
                    break;
                case BinaryOperator.Equal:
                    Codes.Add(InternalCode.Make(Opcode.Equal, token));
                    break;
                case BinaryOperator.NotEqual:
                    Codes.Add(InternalCode.Make(Opcode.NotEqual, token));
                    break;
                case BinaryOperator.GreaterThanOrEqual:
                    Codes.Add(InternalCode.Make(Opcode.GreaterThanOrEqual, token));
                    break;
                case BinaryOperator.LessThanOrEqual:
                    Codes.Add(InternalCode.Make(Opcode.LessThanOrEqual, token));
                    break;
                case BinaryOperator.GreaterThan:
                    Codes.Add(InternalCode.Make(Opcode.GreaterThan, token));
                    break;
                case BinaryOperator.LessThan:
                    Codes.Add(InternalCode.Make(Opcode.LessThan, token));
                    break;
                case BinaryOperator.And:
                    BranchLinkConcat(ref subExprDesc.BranchFalseCodeIndex, exprDesc.BranchFalseCodeIndex);
                    exprDesc = subExprDesc;
                    break;
                case BinaryOperator.Or:
                    BranchLinkConcat(ref subExprDesc.BranchTrueCodeIndex, exprDesc.BranchTrueCodeIndex);
                    exprDesc = subExprDesc;
                    break;
            }

            exprDesc.Kind = ExpressionKind.LocalVariable;
        }

        public void Assign(ref ExpressionDescription exprDesc, bool newLocal, in Token token)
        {
            switch (exprDesc.Kind)
            {
                case ExpressionKind.GlobalVariable:
                    throw new NotImplementedException();
                case ExpressionKind.LocalVariable:
                    if (newLocal)
                        Codes.Add(InternalCode.Make(Opcode.StoreToNewLocal, token));
                    else
                        Codes.Add(InternalCode.Make(Opcode.StoreToLocal, token, (int)exprDesc.Value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(exprDesc)}.{nameof(exprDesc.Kind)}",
                        exprDesc.Kind, null);
            }
        }
    }
}