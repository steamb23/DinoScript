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
        Equal,
        NotEqual,
        GreaterThanOrEqual,
        LessThanOrEqual,
        GreaterThan,
        LessThan
    }

    public enum UnaryOperator
    {
        NoUnaryOperator,
        Not,
        Minus,
        Plus,
    }
}