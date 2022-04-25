using DinoScript.Parser;

namespace DinoScript.Syntax
{
    /// <summary>
    /// 토큰을 나타냅니다.
    /// </summary>
    public class Token
    {
        public Token(TokenType type, string text, string? value, long lines, long columns)
        {
            this.Type = type;
            this.Text = text;
            this.Value = value;
            this.Lines = lines;
            this.Columns = columns;
        }

        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Text = this.Value = value;
        }

        /// <summary>
        /// 토큰의 타입을 가져오거나 초기화합니다.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// 토큰의 값을 가져오거나 초기화합니다.
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// 가공되지 않은 토큰의 원본 값을 가져오거나 초기화합니다.
        /// </summary>
        public string Text { get; } = "";

        /// <summary>
        /// 현재 토큰의 열 위치를 가져옵니다.
        /// </summary>
        public long Columns { get; }

        /// <summary>
        /// 현재 토큰의 줄 위치를 가져옵니다.
        /// </summary>
        public long Lines { get; }
    }
}