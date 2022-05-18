using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DinoScript.Parser;
using DinoScript.Runtime;
using DinoScript.Syntax;

namespace DinoScript.Code.Generator
{
    public partial class CodeGenerator
    {
        private const int NoJump = -1;

        /// <summary>
        /// 분기 코드 및 분기 연결 목록의 목적 인덱스를 변경합니다.
        /// </summary>
        /// <param name="branchCodeIndex"></param>
        /// <param name="targetCodeIndex"></param>
        /// <returns></returns>
        private void PatchBranch(int branchCodeIndex, int targetCodeIndex)
        {
            if (branchCodeIndex == NoJump || targetCodeIndex == NoJump)
            {
                return;
            }

            var next = branchCodeIndex;
            do
            {
                var current = next;
                var code = Codes[current];
                next = (int)code.Operands[0];
                switch (code.Opcode)
                {
                    case Opcode.Branch:
                    case Opcode.BranchIfFalse:
                    case Opcode.BranchIfTrue:
                        Codes[current] = InternalCode.Make(code.Opcode, code.Token, targetCodeIndex);
                        break;
                }
            } while (next != NoJump);
        }

        private int GetBranchTargetIndex(int codeIndex)
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
        private void FixBranch(int codeIndex, int targetCodeIndex)
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

        /// <summary>
        /// 분기 코드 및 분기 연결 목록을 가리키는 <see cref="descCodeIndex"/>를 <see cref="branchCodeIndex"/>가 가리키는 분기 목록에 합칩니다.
        /// </summary>
        /// <param name="branchCodeIndex"></param>
        /// <param name="descCodeIndex"></param>
        private void BranchLinkConcat(ref int branchCodeIndex, int descCodeIndex)
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

        public void ExpressionInitialize(out ExpressionDescription exprDesc, ExpressionKind kind, DinoValue value,
            in Token token)
        {
            var codeIndex = Codes.Count;

            exprDesc = ExpressionDescription.Empty;
            exprDesc.Kind = kind;
            exprDesc.Value = value;
            exprDesc.ValueCodeIndex = codeIndex;


            switch (exprDesc.Value.Type)
            {
                case DinoType.Number:
                    Codes.Add(InternalCode.Make(Opcode.LoadConstantNumber, token, (double)exprDesc.Value));
                    break;
                case DinoType.Integer:
                    Codes.Add(InternalCode.Make(Opcode.LoadConstantInteger, token, (long)exprDesc.Value));
                    break;
                case DinoType.Boolean:
                    Codes.Add(InternalCode.Make(Opcode.LoadConstantBoolean, token, (long)exprDesc.Value));
                    break;
            }
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
                    if (exprDesc.Kind == ExpressionKind.Constant)
                    {
                        switch (exprDesc.Value.Type)
                        {
                            case DinoType.Integer:
                                var longValue = -(long)exprDesc.Value;
                                exprDesc.Value = longValue;
                                Codes[exprDesc.ValueCodeIndex] =
                                    InternalCode.Make(Opcode.LoadConstantInteger, targetCode.Token, longValue);
                                break;
                            case DinoType.Number:
                                var doubleValue = -(double)exprDesc.Value;
                                exprDesc.Value = doubleValue;
                                Codes[exprDesc.ValueCodeIndex] =
                                    InternalCode.Make(Opcode.LoadConstantNumber, targetCode.Token, doubleValue);
                                break;
                            case DinoType.Boolean:
                                throw new SyntaxErrorException(token,
                                    $"A constant of type boolean cannot be prefixed with a '{token.Text}' symbol.");
                            default:
                                throw new SyntaxErrorException(token);
                        }
                    }
                    else
                    {
                        //TODO 변수 타입 체크 기능 추가 필요
                        Codes.Add(InternalCode.Make(Opcode.Negative, token));
                    }

                    break;
                case UnaryOperator.Not:
                    if (exprDesc.Kind == ExpressionKind.Constant)
                    {
                        switch (exprDesc.Value.Type)
                        {
                            case DinoType.Boolean:
                                var boolValue = !(bool)exprDesc.Value;
                                exprDesc.Value = boolValue;
                                Codes[exprDesc.ValueCodeIndex] =
                                    InternalCode.Make(Opcode.LoadConstantBoolean, targetCode.Token, boolValue ? 1 : 0);
                                break;
                            case DinoType.Number:
                                throw new SyntaxErrorException(token,
                                    $"A constant of type number cannot be prefixed with a '{token.Text}' symbol.");
                            default:
                                throw new ArgumentOutOfRangeException(nameof(exprDesc.Value.Type));
                        }
                    }
                    else
                    {
                        //TODO 변수 타입 체크 기능 추가 필요
                        Codes.Add(InternalCode.Make(Opcode.Negative, token));
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
            int codeIndex;
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
        }
    }
}