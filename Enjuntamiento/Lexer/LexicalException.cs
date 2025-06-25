namespace PixelWallE
{
    public class LexicalException : Exception
    {
        public int Line { get; }
        public int Position { get; }

        public LexicalException(string message, int line, int position)
            : base($"{message} at line {line}, position {position}")
        {
            Line = line;
            Position = position;
        }
    }
}