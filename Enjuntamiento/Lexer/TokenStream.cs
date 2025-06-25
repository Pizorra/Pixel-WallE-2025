namespace PixelWallE
{
    public class TokenStream
    {
        private readonly List<Token> tokens;
        private int currentIndex;

        public TokenStream(List<Token> tokens)
        {
            this.tokens = tokens;
            currentIndex = 0;
        }

        public Token? Peek() => currentIndex < tokens.Count ? tokens[currentIndex] : null;
        public Token Next()
        {
            if (currentIndex >= tokens.Count) return null;
            return tokens[currentIndex++];
        }

        public bool Match(TokenType type)
        {
            if (Peek()?.Type == type)
            {
                Next();
                return true;
            }
            return false;
        }

        public Token Consume(TokenType expected, string errorMessage)
        {
            Token token = Next();
            if (token == null || token.Type != expected)
            {
                throw new ParseException(errorMessage, token?.Line ?? 0, token?.Position ?? 0);
            }
            return token;
        }
    }
}