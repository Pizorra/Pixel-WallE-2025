namespace PixelWallE
{
    public class Parser
    {
        private readonly TokenStream tokens;
        public Parser(TokenStream tokens) => this.tokens = tokens;
        public ProgramNode Parse()
        {
            ProgramNode program = new ProgramNode();

            while (true)
            {
                // Saltar líneas vacías y comentarios
                while (tokens.Match(TokenType.EndOfLine) ||
                       tokens.Match(TokenType.Comment)) { }

                Token? token = tokens.Peek();
                if (token == null || token.Type == TokenType.EndOfFile)
                    break;

                ASTNode statement = ParseStatement();
                if (statement != null)
                {
                    program.Statements.Add(statement);
                }
            }

            return program;
        }
        private ASTNode ParseStatement()
        {
            Token? token = tokens.Peek();
            if (token == null) return null;

            switch (token.Type)
            {
                case TokenType.Identifier:
                    tokens.Next(); // Consume el identificador
                    Token? next = tokens.Peek();
                    if (next != null && next.Type == TokenType.Colon)
                    {
                        tokens.Next(); // Consume el colon
                        return new LabelNode
                        {
                            Name = token.Lexeme,
                            Line = token.Line,
                            Position = token.Position
                        };
                    }
                    else if (next != null && next.Type == TokenType.Arrow)
                    {
                        tokens.Next(); // Consume el arrow
                        ExpressionNode expression = ParseExpression();
                        return new AssignmentNode
                        {
                            VariableName = token.Lexeme,
                            Expression = expression,
                            Line = token.Line,
                            Position = token.Position
                        };
                    }
                    else
                    {
                        throw new ParseException("Expected assignment or label", token.Line, token.Position);
                    }

                case TokenType.Spawn:
                case TokenType.Color:
                case TokenType.Size:
                case TokenType.DrawLine:
                case TokenType.DrawCircle:
                case TokenType.DrawRectangle:
                case TokenType.Fill:
                    return ParseInstruction();

                case TokenType.GoTo:
                    return ParseConditionalJump();

                case TokenType.EndOfLine:
                    tokens.Next(); // Skip empty lines
                    return null;

                case TokenType.Comment:
                    tokens.Next(); // Skip comments
                    return null;

                default:
                    throw new ParseException($"Unexpected token: {token.Type}", token.Line, token.Position);
            }
        }

        private LabelNode ParseLabel()
        {
            Token identifier = tokens.Consume(TokenType.Identifier, "Expected label identifier");
            tokens.Consume(TokenType.Colon, "Expected colon after label");
            return new LabelNode { Name = identifier.Lexeme, Line = identifier.Line, Position = identifier.Position };
        }

        private AssignmentNode ParseAssignment()
        {
            Token varToken = tokens.Consume(TokenType.Identifier, "Expected variable name");
            tokens.Consume(TokenType.Arrow, "Expected '<-' in assignment");
            ExpressionNode expression = ParseExpression();

            return new AssignmentNode
            {
                VariableName = varToken.Lexeme,
                Expression = expression,
                Line = varToken.Line,
                Position = varToken.Position
            };
        }

        private InstructionNode ParseInstruction()
        {
            Token instructionToken = tokens.Next();
            // Añadir verificación nula
            if (instructionToken == null)
                throw new ParseException("Unexpected end of input", 0, 0);

            tokens.Consume(TokenType.LeftParen, $"Expected '(' after {instructionToken.Lexeme}");

            InstructionNode node = new InstructionNode
            {
                InstructionType = instructionToken.Type,
                Line = instructionToken.Line,
                Position = instructionToken.Position
            };

            // Parsear argumentos permitiendo espacios
            while (tokens.Peek()?.Type != TokenType.RightParen)
            {
                // Añadir verificación de fin de archivo
                if (tokens.Peek() == null)
                    throw new ParseException("Unexpected end of input", node.Line, node.Position);

                node.Arguments.Add(ParseExpression());

                // Manejar coma o fin de argumentos
                if (tokens.Peek()?.Type == TokenType.Comma)
                {
                    tokens.Next();
                }
            }

            tokens.Consume(TokenType.RightParen, $"Expected ')' after {instructionToken.Lexeme} arguments");
            return node;
        }
        private ConditionalJumpNode ParseConditionalJump()
        {
            Token gotoToken = tokens.Consume(TokenType.GoTo, "Expected 'GoTo'");
            tokens.Consume(TokenType.LeftBracket, "Expected '[' after GoTo");

            Token labelToken = tokens.Consume(TokenType.Identifier, "Expected label identifier");
            tokens.Consume(TokenType.RightBracket, "Expected ']' after label");
            tokens.Consume(TokenType.LeftParen, "Expected '(' before condition");

            ExpressionNode condition = ParseExpression();
            tokens.Consume(TokenType.RightParen, "Expected ')' after condition");

            return new ConditionalJumpNode
            {
                Label = labelToken.Lexeme,
                Condition = condition,
                Line = gotoToken.Line,
                Position = gotoToken.Position
            };
        }

        private ExpressionNode ParseExpression(int precedence = 0)
        {
            ExpressionNode left = ParsePrimary();

            while (true)
            {
                Token? opToken = tokens.Peek();
                if (opToken == null || !IsBinaryOperator(opToken.Type)) break;

                int opPrecedence = GetPrecedence(opToken.Type);
                if (opPrecedence <= precedence) break;

                tokens.Next();
                ExpressionNode right = ParseExpression(opPrecedence);

                left = new BinaryExpressionNode
                {
                    Left = left,
                    Operator = opToken.Type,
                    Right = right,
                    Line = opToken.Line,
                    Position = opToken.Position
                };
            }

            return left;
        }

        private ExpressionNode ParsePrimary()
        {
            Token? token = tokens.Peek();
            if (token == null) throw new ParseException("Unexpected end of input", 0, 0);

            if (token.Type >= TokenType.GetActualX && token.Type <= TokenType.IsCanvasColor)
            {
                return ParseFunctionCall();
            }

            switch (token.Type)
            {
                case TokenType.Number:
                    tokens.Next();
                    return new LiteralNode
                    {
                        Value = int.Parse(token.Lexeme),
                        ValueType = TokenType.Number,
                        Line = token.Line,
                        Position = token.Position
                    };

                case TokenType.Boolean:
                    tokens.Next();
                    return new LiteralNode
                    {
                        Value = bool.Parse(token.Lexeme),
                        ValueType = TokenType.Boolean,
                        Line = token.Line,
                        Position = token.Position
                    };

                case TokenType.String:
                    tokens.Next();
                    return new LiteralNode
                    {
                        Value = token.Lexeme,
                        ValueType = TokenType.String,
                        Line = token.Line,
                        Position = token.Position
                    };

                case TokenType.Identifier:
                    // Cambio importante: verificar si es una función integrada
                    if (IsBuiltInFunction(token.Type))
                    {
                        return ParseFunctionCall();
                    }
                    tokens.Next();
                    return new VariableNode
                    {
                        Name = token.Lexeme,
                        Line = token.Line,
                        Position = token.Position
                    };

                case TokenType.LeftParen:
                    tokens.Next();
                    ExpressionNode expr = ParseExpression();
                    tokens.Consume(TokenType.RightParen, "Expected ')' after expression");
                    return expr;

                case TokenType.Not:
                case TokenType.Minus:
                    tokens.Next();
                    return new UnaryExpressionNode
                    {
                        Operator = token.Type,
                        Operand = ParsePrimary(),
                        Line = token.Line,
                        Position = token.Position
                    };

                default:
                    throw new ParseException($"Unexpected token: {token.Type}", token.Line, token.Position);
            }
        }
        private bool IsBuiltInFunction(TokenType type)
        {
            return type >= TokenType.GetActualX && type <= TokenType.IsCanvasColor;
        }

        private FunctionCallNode ParseFunctionCall()
        {
            Token funcToken = tokens.Next();

            tokens.Consume(TokenType.LeftParen, "Expected '(' after function name");

            FunctionCallNode node = new FunctionCallNode
            {
                FunctionName = funcToken.Type,
                Line = funcToken.Line,
                Position = funcToken.Position
            };

            while (tokens.Peek()?.Type != TokenType.RightParen)
            {
                node.Arguments.Add(ParseExpression());

                if (tokens.Peek()?.Type == TokenType.Comma)
                {
                    tokens.Next();
                }
                else if (tokens.Peek()?.Type != TokenType.RightParen)
                {
                    throw new ParseException("Expected ',' or ')' in function arguments", funcToken.Line, funcToken.Position);
                }
            }

            tokens.Consume(TokenType.RightParen, "Expected ')' after function arguments");
            return node;
        }

        private bool IsBinaryOperator(TokenType type)
        {
            return type == TokenType.Plus || type == TokenType.Minus ||
                   type == TokenType.Multiply || type == TokenType.Divide ||
                   type == TokenType.Modulo || type == TokenType.Power ||
                   type == TokenType.Equal || type == TokenType.NotEqual ||
                   type == TokenType.Greater || type == TokenType.GreaterEqual ||
                   type == TokenType.Less || type == TokenType.LessEqual ||
                   type == TokenType.And || type == TokenType.Or;
        }

        private int GetPrecedence(TokenType type)
        {
            switch (type)
            {
                case TokenType.Or:
                    return 1;
                case TokenType.And:
                    return 2;
                case TokenType.Equal:
                case TokenType.NotEqual:
                case TokenType.Less:
                case TokenType.LessEqual:
                case TokenType.Greater:
                case TokenType.GreaterEqual:
                    return 3;
                case TokenType.Plus:
                case TokenType.Minus:
                    return 4;
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Modulo:
                    return 5;
                case TokenType.Power:
                    return 6;
                default:
                    return 0;
            }
        }
    }
}