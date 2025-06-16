 namespace PixelWallE{
 public abstract class ASTNode
    {
        public int Line { get; set; }
        public int Position { get; set; }
    }

    public class ProgramNode : ASTNode
    {
        public List<ASTNode> Statements { get; } = new List<ASTNode>();
    }

    public class AssignmentNode : ASTNode
    {
        public string ?VariableName { get; set; }
        public ExpressionNode ?Expression { get; set; }
    }

    public class InstructionNode : ASTNode
    {
        public TokenType InstructionType { get; set; }
        public List<ExpressionNode> Arguments { get; } = new List<ExpressionNode>();
    }

    public class ConditionalJumpNode : ASTNode
    {
        public string ?Label { get; set; }
        public ExpressionNode ?Condition { get; set; }
    }

    public class LabelNode : ASTNode
    {
        public string ?Name { get; set; }
    }

    public abstract class ExpressionNode : ASTNode { }

    public class BinaryExpressionNode : ExpressionNode
    {
        public TokenType Operator { get; set; }
        public ExpressionNode ?Left { get; set; }
        public ExpressionNode ?Right { get; set; }
    }

    public class UnaryExpressionNode : ExpressionNode
    {
        public TokenType Operator { get; set; }
        public ExpressionNode ?Operand { get; set; }
    }

    public class FunctionCallNode : ExpressionNode
    {
        public TokenType FunctionName { get; set; }
        public List<ExpressionNode> Arguments { get; } = new List<ExpressionNode>();
    }

    public class LiteralNode : ExpressionNode
    {
        public object ?Value { get; set; }
        public TokenType ValueType { get; set; }
    }

    public class VariableNode : ExpressionNode
    {
        public string ?Name { get; set; }
    }
 }