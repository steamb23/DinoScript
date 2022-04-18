namespace DinoScript.Parser;

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