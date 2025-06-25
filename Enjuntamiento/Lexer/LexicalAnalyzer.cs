namespace PixelWallE
{
    public class LexicalAnalyzer
    {
        private readonly string source;
        private int currentPosition;
        private int currentLine;
        private int lineStartPosition;

        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"spawn", TokenType.Spawn},
            {"color", TokenType.Color},
            {"size", TokenType.Size},
            {"drawline", TokenType.DrawLine},
            {"drawcircle", TokenType.DrawCircle},
            {"drawrectangle", TokenType.DrawRectangle},
            {"fill", TokenType.Fill},
            {"goto", TokenType.GoTo},
            {"getactualx", TokenType.GetActualX},
            {"getactualy", TokenType.GetActualY},
            {"getcanvassize", TokenType.GetCanvasSize},
            {"getcolorcount", TokenType.GetColorCount},
            {"isbrushcolor", TokenType.IsBrushColor},
            {"isbrushsize", TokenType.IsBrushSize},
            {"iscanvascolor", TokenType.IsCanvasColor},
            {"true", TokenType.Boolean},
            {"false", TokenType.Boolean},
            {"and", TokenType.And},
            {"or", TokenType.Or},
            {"not", TokenType.Not}
        };

        private static readonly Dictionary<string, TokenType> Operators = new Dictionary<string, TokenType>
        {
            {"+", TokenType.Plus},
            {"-", TokenType.Minus},
            {"*", TokenType.Multiply},
            {"/", TokenType.Divide},
            {"%", TokenType.Modulo},
            {"^", TokenType.Power},
            {"==", TokenType.Equal},
            {"!=", TokenType.NotEqual},
            {">", TokenType.Greater},
            {">=", TokenType.GreaterEqual},
            {"<", TokenType.Less},
            {"<=", TokenType.LessEqual},
            {"&&", TokenType.And},
            {"||", TokenType.Or},
            {"<-", TokenType.Arrow},
            {":", TokenType.Colon},
            {"(", TokenType.LeftParen},
            {")", TokenType.RightParen},
            {"[", TokenType.LeftBracket},
            {"]", TokenType.RightBracket},
            {",", TokenType.Comma}
        };

        public LexicalAnalyzer(string source)
        {
            this.source = source;
            currentPosition = 0;
            currentLine = 1;
            lineStartPosition = 0;
        }

        public TokenStream Analyze()
        {
            List<Token> tokens = new List<Token>();
            currentPosition = 0;
            currentLine = 1;
            lineStartPosition = 0;

            while (currentPosition < source.Length)
            {
                char current = source[currentPosition];

                // Manejo de retornos de carro
                if (current == '\r')
                {
                    currentPosition++;
                    if (currentPosition < source.Length && source[currentPosition] == '\n')
                    {
                        currentPosition++;
                    }
                    currentLine++;
                    lineStartPosition = currentPosition;
                    continue;
                }

                // Manejo de saltos de línea
                if (current == '\n')
                {
                    tokens.Add(new Token(TokenType.EndOfLine, "EOL", currentLine, currentPosition - lineStartPosition));
                    currentLine++;
                    currentPosition++;
                    lineStartPosition = currentPosition;
                    continue;
                }

                // Ignorar espacios en blanco (excepto saltos de línea)
                if (char.IsWhiteSpace(current))
                {
                    currentPosition++;
                    continue;
                }

                // Comentarios
                if (current == '/' && currentPosition + 1 < source.Length && source[currentPosition + 1] == '/')
                {
                    string comment = ScanComment();
                    tokens.Add(new Token(TokenType.Comment, comment, currentLine, currentPosition - lineStartPosition));
                    continue;
                }

                // Números
                if (char.IsDigit(current))
                {
                    string number = ScanNumber();
                    tokens.Add(new Token(TokenType.Number, number, currentLine, currentPosition - lineStartPosition));
                    continue;
                }

                // Identificadores
                if (char.IsLetter(current) || current == '_')
                {
                    string identifier = ScanIdentifier();
                    if (Keywords.TryGetValue(identifier.ToLower(), out TokenType keywordType))
                    {
                        tokens.Add(new Token(keywordType, identifier, currentLine, currentPosition - lineStartPosition));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Identifier, identifier, currentLine, currentPosition - lineStartPosition));
                    }
                    continue;
                }

                // Strings
                if (current == '"')
                {
                    string str = ScanString();
                    tokens.Add(new Token(TokenType.String, str, currentLine, currentPosition - lineStartPosition));
                    continue;
                }

                // Operadores
                TokenType? op = ScanOperator();
                if (op.HasValue)
                {
                    tokens.Add(new Token(op.Value, source.Substring(currentPosition, 1), currentLine, currentPosition - lineStartPosition));
                    continue;
                }

                // Carácter no reconocido
                throw new LexicalException($"Unrecognized character: '{current}'", currentLine, currentPosition - lineStartPosition);
            }

            tokens.Add(new Token(TokenType.EndOfFile, "EOF", currentLine, currentPosition - lineStartPosition));
            return new TokenStream(tokens);
        }

        private string ScanComment()
        {
            int start = currentPosition;
            while (currentPosition < source.Length && source[currentPosition] != '\n' && source[currentPosition] != '\r')
            {
                currentPosition++;
            }
            return source.Substring(start, currentPosition - start);
        }

        private string ScanNumber()
        {
            int start = currentPosition;
            while (currentPosition < source.Length && char.IsDigit(source[currentPosition]))
            {
                currentPosition++;
            }
            return source.Substring(start, currentPosition - start);
        }

        private string ScanIdentifier()
        {
            int start = currentPosition;
            while (currentPosition < source.Length &&
                  (char.IsLetterOrDigit(source[currentPosition]) || source[currentPosition] == '_'))
            {
                currentPosition++;
            }
            return source.Substring(start, currentPosition - start);
        }

        private string ScanString()
        {
            currentPosition++; // Saltar comilla inicial
            int start = currentPosition;
            while (currentPosition < source.Length && source[currentPosition] != '"')
            {
                currentPosition++;
            }
            if (currentPosition >= source.Length)
            {
                throw new LexicalException("Unterminated string literal", currentLine, currentPosition - lineStartPosition);
            }
            string str = source.Substring(start, currentPosition - start);
            currentPosition++; // Saltar comilla final
            return str;
        }

        private TokenType? ScanOperator()
        {
            // Verificar operadores de 2 caracteres primero
            if (currentPosition + 1 < source.Length) // Cambio clave aquí
            {
                string twoCharOp = source.Substring(currentPosition, 2);
                if (Operators.TryGetValue(twoCharOp, out TokenType opType))
                {
                    currentPosition += 2;
                    return opType;
                }
            }

            // Verificar operadores de 1 carácter
            if (currentPosition < source.Length)
            {
                string oneCharOp = source.Substring(currentPosition, 1);
                if (Operators.TryGetValue(oneCharOp, out TokenType type))
                {
                    currentPosition++;
                    return type;
                }
            }

            return null;
        }
    }
}