namespace PixelWallE
{
    public class Interpreter
    {
        public Interpreter(IVisualizer visualizer)
        {
            this.visualizer = visualizer;
        }
        private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
        private Dictionary<string, int> labels = new Dictionary<string, int>();
        private List<ASTNode> statements;
        private int currentStatement = 0;

        // Canvas state
        private int canvasSize = 100;
        private int posX, posY;
        private Color currentColor = Color.Transparent;
        private int brushSize = 1;
        private Color[,] canvas;
        private bool hasSpawned = false;
        private readonly IVisualizer visualizer;


        public void Execute(ProgramNode program)
        {

            statements = program.Statements;
            currentStatement = 0;
            hasSpawned = false;

            // First pass: collect labels
            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i] is LabelNode label)
                {
                    labels[label.Name] = i;
                }
            }

            // Second pass: execute
            while (currentStatement < statements.Count)
            {
                ExecuteNode(statements[currentStatement]);
                currentStatement++;
            }
        }

        private void ExecuteNode(ASTNode node)
        {
            try
            {
                switch (node)
                {
                    case AssignmentNode assignment:
                        ExecuteAssignment(assignment);
                        break;
                    case InstructionNode instruction:
                        ExecuteInstruction(instruction);
                        break;
                    case ConditionalJumpNode jump:
                        ExecuteConditionalJump(jump);
                        break;
                    case LabelNode:
                        // Labels are already processed, skip execution
                        break;
                    default:
                        throw new RuntimeException($"Unknown node type: {node.GetType().Name}", node.Line, node.Position);
                }
            }
            catch (RuntimeException ex)
            {
                throw new Exception($"Runtime error at line {ex.Line}, position {ex.Position}: {ex.Message}");
            }
        }

        private void ExecuteAssignment(AssignmentNode assignment)
        {
            Variable value = EvaluateExpression(assignment.Expression);
            variables[assignment.VariableName] = value;
        }

        private void ExecuteInstruction(InstructionNode instruction)
        {
            switch (instruction.InstructionType)
            {
                case TokenType.Spawn:
                    ExecuteSpawn(instruction);
                    break;
                case TokenType.Color:
                    ExecuteColor(instruction);
                    break;
                case TokenType.Size:
                    ExecuteSize(instruction);
                    break;
                case TokenType.DrawLine:
                    ExecuteDrawLine(instruction);
                    break;
                case TokenType.DrawCircle:
                    ExecuteDrawCircle(instruction);
                    break;
                case TokenType.DrawRectangle:
                    ExecuteDrawRectangle(instruction);
                    break;
                case TokenType.Fill:
                    ExecuteFill(instruction);
                    break;
                default:
                    throw new RuntimeException($"Unknown instruction: {instruction.InstructionType}", instruction.Line, instruction.Position);
            }
        }

        private void ExecuteSpawn(InstructionNode instruction)
        {
            if (hasSpawned)
                throw new RuntimeException("Spawn can only be called once at the beginning", instruction.Line, instruction.Position);

            if (instruction.Arguments.Count != 2)
                throw new RuntimeException("Spawn requires exactly 2 arguments", instruction.Line, instruction.Position);

            int x = EvaluateNumeric(instruction.Arguments[0]);
            int y = EvaluateNumeric(instruction.Arguments[1]);

            if (x < 0 || x >= canvasSize || y < 0 || y >= canvasSize)
                throw new RuntimeException($"Spawn coordinates ({x}, {y}) are outside canvas", instruction.Line, instruction.Position);


            posX = x;
            posY = y;
            hasSpawned = true;

            //Console.WriteLine($"Visual: Wall-E spawned at ({x}, {y})");
            visualizer.ExecuteVisualSpawn(x, y);
        }

        private void ExecuteColor(InstructionNode instruction)
        {
            CheckSpawnCalled(instruction);

            if (instruction.Arguments.Count != 1)
                throw new RuntimeException("Color requires exactly 1 argument", instruction.Line, instruction.Position);

            string colorStr = EvaluateString(instruction.Arguments[0]);

            if (!Enum.TryParse<Color>(colorStr, true, out Color color))
                throw new RuntimeException($"Invalid color: {colorStr}", instruction.Line, instruction.Position);

            currentColor = color;
            //Console.WriteLine($"Visual: Color changed to {color}");
            visualizer.ExecuteVisualColor(color);
        }

        private void ExecuteSize(InstructionNode instruction)
        {
            CheckSpawnCalled(instruction);
            if (instruction.Arguments.Count != 1)
                throw new RuntimeException("Size requires exactly 1 argument",
                                          instruction.Line, instruction.Position);

            int size = EvaluateNumeric(instruction.Arguments[0]);
            if (size <= 0)
                throw new RuntimeException("Brush size must be positive",
                                          instruction.Line, instruction.Position);

            brushSize = size % 2 == 0 ? size - 1 : size; // Ensure odd size
            //Console.WriteLine($"Visual: Brush size changed to {brushSize}");
            visualizer.ExecuteVisualBrushSize(brushSize);
        }

        private void ExecuteDrawLine(InstructionNode instruction)
        {
            CheckSpawnCalled(instruction);
            if (instruction.Arguments.Count != 3)
                throw new RuntimeException("DrawLine requires exactly 3 arguments",
                                          instruction.Line, instruction.Position);

            int dirX = EvaluateNumeric(instruction.Arguments[0]);
            int dirY = EvaluateNumeric(instruction.Arguments[1]);
            int distance = EvaluateNumeric(instruction.Arguments[2]);

            if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1 || (dirX == 0 && dirY == 0))
                throw new RuntimeException("Invalid direction vector",
                                          instruction.Line, instruction.Position);

            if (distance <= 0)
                throw new RuntimeException("Distance must be positive",
                                          instruction.Line, instruction.Position);

            int startX = posX;
            int startY = posY;
            int endX = posX + dirX * distance;
            int endY = posY + dirY * distance;

            if (endX < 0 || endX >= canvasSize || endY < 0 || endY >= canvasSize)
                throw new RuntimeException("Drawing would go outside canvas bounds",
                                          instruction.Line, instruction.Position);

            // Actual drawing logic would go here
            //Console.WriteLine($"Visual: Drawing line from ({startX},{startY}) to ({endX},{endY}) with color {currentColor} and size {brushSize}");
            visualizer.ExecuteVisualDrawLine(startX, startY, endX, endY, currentColor, brushSize);
            // Update position
            posX = endX;
            posY = endY;
        }

        private void ExecuteDrawCircle(InstructionNode instruction)
        {
            CheckSpawnCalled(instruction);
            if (instruction.Arguments.Count != 3)
                throw new RuntimeException("DrawCircle requires exactly 3 arguments",
                                          instruction.Line, instruction.Position);

            int dirX = EvaluateNumeric(instruction.Arguments[0]);
            int dirY = EvaluateNumeric(instruction.Arguments[1]);
            int radius = EvaluateNumeric(instruction.Arguments[2]);

            if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1 || (dirX == 0 && dirY == 0))
                throw new RuntimeException("Invalid direction vector",
                                          instruction.Line, instruction.Position);

            if (radius <= 0)
                throw new RuntimeException("Radius must be positive",
                                          instruction.Line, instruction.Position);

            int centerX = posX + dirX * radius;
            int centerY = posY + dirY * radius;

            if (centerX < 0 || centerX >= canvasSize || centerY < 0 || centerY >= canvasSize)
                throw new RuntimeException("Circle center would be outside canvas bounds",
                                          instruction.Line, instruction.Position);

            // Actual circle drawing logic would go here
            visualizer.ExecuteVisualDrawCircle(centerX, centerY, radius, currentColor, brushSize);
            posX = centerX;
            posY = centerY;
            //Console.WriteLine($"Visual: Drawing circle at ({centerX},{centerY}) with radius {radius}, color {currentColor} and size {brushSize}");
        }

        private void ExecuteDrawRectangle(InstructionNode instruction)
        {
            CheckSpawnCalled(instruction);
            if (instruction.Arguments.Count != 5)
                throw new RuntimeException("DrawRectangle requires exactly 5 arguments",
                                          instruction.Line, instruction.Position);

            int dirX = EvaluateNumeric(instruction.Arguments[0]);
            int dirY = EvaluateNumeric(instruction.Arguments[1]);
            int distance = EvaluateNumeric(instruction.Arguments[2]);
            int width = EvaluateNumeric(instruction.Arguments[3]);
            int height = EvaluateNumeric(instruction.Arguments[4]);

            if (Math.Abs(dirX) > 1 || Math.Abs(dirY) > 1 || (dirX == 0 && dirY == 0))
                throw new RuntimeException("Invalid direction vector",
                                          instruction.Line, instruction.Position);

            if (distance <= 0 || width <= 0 || height <= 0)
                throw new RuntimeException("Distance, width and height must be positive",
                                          instruction.Line, instruction.Position);

            int startX = posX + dirX * distance;
            int startY = posY + dirY * distance;
            int endX = startX + width - 1;
            int endY = startY + height - 1;

            if (startX < 0 || startX >= canvasSize || startY < 0 || startY >= canvasSize ||
                endX < 0 || endX >= canvasSize || endY < 0 || endY >= canvasSize)
            {
                throw new RuntimeException("Rectangle would go outside canvas bounds",
                                          instruction.Line, instruction.Position);
            }

            // Actual rectangle drawing logic would go here
            // Console.WriteLine($"Visual: Drawing rectangle at ({startX},{startY}) with width {width}, height {height}, color {currentColor} and size {brushSize}");
            visualizer.ExecuteVisualDrawRectangle(
       startX, startY,
       width, height,
       currentColor,
       brushSize
   );

            // Actualizar posición: esquina superior izquierda del rectángulo
            posX = startX;
            posY = startY;
        }

        private void ExecuteFill(InstructionNode instruction)
        {
            CheckSpawnCalled(instruction);
            if (instruction.Arguments.Count != 0)
                throw new RuntimeException("Fill takes no arguments",
                                          instruction.Line, instruction.Position);

            // Actual fill algorithm would go here
            // Console.WriteLine($"Visual: Filling area starting from ({posX},{posY}) with color {currentColor}");
            visualizer.ExecuteVisualFill(posX, posY, currentColor);
        }



        private void ExecuteConditionalJump(ConditionalJumpNode jump)
        {
            bool condition = EvaluateBoolean(jump.Condition);
            if (condition)
            {
                if (!labels.TryGetValue(jump.Label, out int targetIndex))
                    throw new RuntimeException($"Label '{jump.Label}' not found", jump.Line, jump.Position);

                currentStatement = targetIndex;
            }
        }

        private Variable EvaluateExpression(ExpressionNode expression)
        {
            return expression switch
            {
                BinaryExpressionNode binary => EvaluateBinary(binary),
                UnaryExpressionNode unary => EvaluateUnary(unary),
                FunctionCallNode func => EvaluateFunction(func),
                LiteralNode literal => EvaluateLiteral(literal),
                VariableNode varNode => EvaluateVariable(varNode),
                _ => throw new RuntimeException("Unknown expression type", expression.Line, expression.Position)
            };
        }

        private int EvaluateNumeric(ExpressionNode node)
        {
            Variable result = EvaluateExpression(node);
            if (result.IsBoolean)
                throw new RuntimeException("Expected numeric value", node.Line, node.Position);
            return result.NumericValue;
        }

        private bool EvaluateBoolean(ExpressionNode node)
        {
            Variable result = EvaluateExpression(node);
            if (!result.IsBoolean)
                throw new RuntimeException("Expected boolean value", node.Line, node.Position);
            return result.BoolValue;
        }

        private string EvaluateString(ExpressionNode node)
        {
            Variable result = EvaluateExpression(node);
            if (result.ValueType != TokenType.String)
                throw new RuntimeException("Expected string value", node.Line, node.Position);
            return result.StringValue;
        }

        private Variable EvaluateBinary(BinaryExpressionNode binary)
        {
            Variable left = EvaluateExpression(binary.Left);
            Variable right = EvaluateExpression(binary.Right);

            return binary.Operator switch
            {
                TokenType.Plus => new Variable
                {
                    NumericValue = left.NumericValue + right.NumericValue,
                    IsBoolean = false
                },
                TokenType.Minus => new Variable
                {
                    NumericValue = left.NumericValue - right.NumericValue,
                    IsBoolean = false
                },
                TokenType.Multiply => new Variable
                {
                    NumericValue = left.NumericValue * right.NumericValue,
                    IsBoolean = false
                },
                TokenType.Divide => new Variable
                {
                    NumericValue = left.NumericValue / right.NumericValue,
                    IsBoolean = false
                },
                TokenType.Modulo => new Variable
                {
                    NumericValue = left.NumericValue % right.NumericValue,
                    IsBoolean = false
                },
                TokenType.Power => new Variable
                {
                    NumericValue = (int)Math.Pow(left.NumericValue, right.NumericValue),
                    IsBoolean = false
                },
                TokenType.Equal => new Variable
                {
                    BoolValue = left.NumericValue == right.NumericValue,
                    IsBoolean = true
                },
                TokenType.NotEqual => new Variable
                {
                    BoolValue = left.NumericValue != right.NumericValue,
                    IsBoolean = true
                },
                TokenType.Less => new Variable
                {
                    BoolValue = left.NumericValue < right.NumericValue,
                    IsBoolean = true
                },
                TokenType.LessEqual => new Variable
                {
                    BoolValue = left.NumericValue <= right.NumericValue,
                    IsBoolean = true
                },
                TokenType.Greater => new Variable
                {
                    BoolValue = left.NumericValue > right.NumericValue,
                    IsBoolean = true
                },
                TokenType.GreaterEqual => new Variable
                {
                    BoolValue = left.NumericValue >= right.NumericValue,
                    IsBoolean = true
                },
                TokenType.And => new Variable
                {
                    BoolValue = left.BoolValue && right.BoolValue,
                    IsBoolean = true
                },
                TokenType.Or => new Variable
                {
                    BoolValue = left.BoolValue || right.BoolValue,
                    IsBoolean = true
                },
                _ => throw new RuntimeException($"Unsupported operator: {binary.Operator}",
                                                binary.Line, binary.Position)
            };
        }

        private Variable EvaluateUnary(UnaryExpressionNode unary)
        {
            Variable operand = EvaluateExpression(unary.Operand);

            return unary.Operator switch
            {
                TokenType.Minus => new Variable
                {
                    NumericValue = -operand.NumericValue,
                    IsBoolean = false
                },
                TokenType.Not => new Variable
                {
                    BoolValue = !operand.BoolValue,
                    IsBoolean = true
                },
                _ => throw new RuntimeException($"Unsupported unary operator: {unary.Operator}",
                                               unary.Line, unary.Position)
            };
        }

        private Variable EvaluateFunction(FunctionCallNode func)
        {
            switch (func.FunctionName)
            {
                case TokenType.GetActualX:
                    CheckSpawnCalled(func);
                    return new Variable
                    {
                        NumericValue = posX,
                        IsBoolean = false
                    };

                case TokenType.GetActualY:
                    CheckSpawnCalled(func);
                    return new Variable
                    {
                        NumericValue = posY,
                        IsBoolean = false
                    };

                case TokenType.GetCanvasSize:
                    return new Variable
                    {
                        NumericValue = canvasSize,
                        IsBoolean = false
                    };

                case TokenType.GetColorCount:
                    return EvaluateGetColorCount(func);

                case TokenType.IsBrushColor:
                    return EvaluateIsBrushColor(func);

                case TokenType.IsBrushSize:
                    return EvaluateIsBrushSize(func);

                case TokenType.IsCanvasColor:
                    return EvaluateIsCanvasColor(func);

                default:
                    throw new RuntimeException($"Unknown function: {func.FunctionName}",
                                              func.Line, func.Position);
            }
        }

        private Variable EvaluateGetColorCount(FunctionCallNode func)
        {
            if (func.Arguments.Count != 5)
                throw new RuntimeException("GetColorCount requires 5 parameters",
                                          func.Line, func.Position);

            string colorStr = EvaluateString(func.Arguments[0]);
            if (!Enum.TryParse<Color>(colorStr, true, out Color color))
                throw new RuntimeException($"Invalid color: {colorStr}",
                                          func.Line, func.Position);

            int x1 = EvaluateNumeric(func.Arguments[1]);
            int y1 = EvaluateNumeric(func.Arguments[2]);
            int x2 = EvaluateNumeric(func.Arguments[3]);
            int y2 = EvaluateNumeric(func.Arguments[4]);

            if (x1 < 0 || x1 >= canvasSize || y1 < 0 || y1 >= canvasSize ||
                x2 < 0 || x2 >= canvasSize || y2 < 0 || y2 >= canvasSize)
            {
                return new Variable
                {
                    NumericValue = 0,
                    IsBoolean = false
                };
            }

            int count = 0;
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    // Lógica para contar colores en el canvas
                    // (Implementación real dependería de tu estructura de datos)
                    if (canvas[x, y] == color)
                        count++;
                }
            }

            return new Variable
            {
                NumericValue = count,
                IsBoolean = false
            };
        }

        private Variable EvaluateIsBrushColor(FunctionCallNode func)
        {
            if (func.Arguments.Count != 1)
                throw new RuntimeException("IsBrushColor requires 1 parameter",
                                          func.Line, func.Position);

            string colorStr = EvaluateString(func.Arguments[0]);
            if (!Enum.TryParse<Color>(colorStr, true, out Color color))
                throw new RuntimeException($"Invalid color: {colorStr}",
                                          func.Line, func.Position);

            return new Variable
            {
                NumericValue = currentColor == color ? 1 : 0,
                IsBoolean = false
            };
        }

        private Variable EvaluateIsBrushSize(FunctionCallNode func)
        {
            if (func.Arguments.Count != 1)
                throw new RuntimeException("IsBrushSize requires 1 parameter",
                                          func.Line, func.Position);

            int size = EvaluateNumeric(func.Arguments[0]);
            return new Variable
            {
                NumericValue = brushSize == size ? 1 : 0,
                IsBoolean = false
            };
        }

        private Variable EvaluateIsCanvasColor(FunctionCallNode func)
        {
            if (func.Arguments.Count != 3)
                throw new RuntimeException("IsCanvasColor requires 3 parameters",
                                          func.Line, func.Position);

            string colorStr = EvaluateString(func.Arguments[0]);
            if (!Enum.TryParse<Color>(colorStr, true, out Color color))
                throw new RuntimeException($"Invalid color: {colorStr}",
                                          func.Line, func.Position);

            int vertical = EvaluateNumeric(func.Arguments[1]);
            int horizontal = EvaluateNumeric(func.Arguments[2]);

            int checkX = posX + horizontal;
            int checkY = posY + vertical;

            if (checkX < 0 || checkX >= canvasSize || checkY < 0 || checkY >= canvasSize)
            {
                return new Variable
                {
                    NumericValue = 0,
                    IsBoolean = false
                };
            }

            return new Variable
            {
                NumericValue = canvas[checkX, checkY] == color ? 1 : 0,
                IsBoolean = false
            };
        }

        private Variable EvaluateLiteral(LiteralNode literal)
        {
            return literal.ValueType switch
            {
                TokenType.Number => new Variable
                {
                    NumericValue = (int)literal.Value,
                    IsBoolean = false
                },
                TokenType.Boolean => new Variable
                {
                    BoolValue = (bool)literal.Value,
                    IsBoolean = true
                },
                TokenType.String => new Variable
                {
                    StringValue = (string)literal.Value,
                    ValueType = TokenType.String
                },
                _ => throw new RuntimeException($"Unsupported literal type: {literal.ValueType}",
                                              literal.Line, literal.Position)
            };
        }

        private Variable EvaluateVariable(VariableNode varNode)
        {
            if (!variables.TryGetValue(varNode.Name, out Variable value))
                throw new RuntimeException($"Variable '{varNode.Name}' not defined",
                                          varNode.Line, varNode.Position);
            return value;
        }




        private void CheckSpawnCalled(ASTNode node)
        {
            if (!hasSpawned)
                throw new RuntimeException("Must call Spawn before any other instructions", node.Line, node.Position);
        }


    }
}