namespace DinoScript.Parser
{
    public enum BinaryOperator
    {
        NoBinaryOperator,
        // 수식 연산자
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        // 비교 연산자
        Equal,
        NotEqual,
        GreaterThanOrEqual,
        LessThanOrEqual,
        GreaterThan,
        LessThan,
        // 논리 연산자
        And,
        Or,
        // 조건부 연산자
        Conditional,
        ConditionalElse,
        // 할당 연산자
        Assign
    }

    public enum UnaryOperator
    {
        NoUnaryOperator,
        Not,
        Minus,
        Plus,
    }
}