using DinoScript.Syntax;

namespace DinoScript.Parser;

public class SyntaxErrorException : Exception
{
    public SyntaxErrorException(Token? token)
        : this(token, token == null
            ? "No token."
            : $"'{token.Text}' is invalid token.", null)
    {
        Token = token;
    }

    public SyntaxErrorException(Token? token, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Token = token;
    }

    public override string Message =>
        Token == null ? base.Message : $"({Token.Lines}, {Token.Columns}) : {base.Message}";

    public Token? Token { get; }
}