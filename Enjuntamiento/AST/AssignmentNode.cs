namespace PixelWallE
{
    public class AssignmentNode : ASTNode
    {
        public string? VariableName { get; set; }
        public ExpressionNode? Expression { get; set; }
    }
}