namespace PixelWallE
{
    public class ConditionalJumpNode : ASTNode
    {
        public string? Label { get; set; }
        public ExpressionNode? Condition { get; set; }
    }
}