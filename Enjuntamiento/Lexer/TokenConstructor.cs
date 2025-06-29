namespace PixelWallE
{
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public int Line { get; }
        public int Position { get; }

        public Token(TokenType type, string lexeme, int line, int position)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Position = position;
        }

        public override string ToString() => $"{Type}: '{Lexeme}' at {Line}:{Position}";
    }
}