namespace DinoScript.Parser
{
    /// <summary>
    /// 식의 타입을 나타냅니다. 이 열거형이 가진 값은 연산에 대한 우선순위를 나타냅니다.
    /// </summary>
    public enum ExpressionTypes : uint
    {
        NoExpression,
        Group,
        Primary,
        Unary,
        Multiplicative,
        Addictive,
        Comparison,
        Equality,
        LogicalAnd,
        LogicalOr,
        Conditional,
        Assign
    }
}