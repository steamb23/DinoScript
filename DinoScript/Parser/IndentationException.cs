using System;
using DinoScript.Syntax;

namespace DinoScript.Parser
{
    public class IndentationException : SyntaxErrorException
    {
        public int CurrentIndentCount { get; }

        public IndentationException(
            Token? token,
            int currentIndentCount)
            : this(token, currentIndentCount, $"Invalid indentation count.")
        {
        }

        public IndentationException(
            Token? token,
            int currentIndentCount,
            string message,
            Exception? innerException = null)
            : base(token, message, innerException)
        {
            CurrentIndentCount = currentIndentCount;
        }
    }
}