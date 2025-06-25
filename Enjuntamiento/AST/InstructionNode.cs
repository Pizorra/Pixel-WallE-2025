namespace PixelWallE
{

    public class InstructionNode : ASTNode
    {
        public TokenType InstructionType { get; set; }
        public List<ExpressionNode> Arguments { get; } = new List<ExpressionNode>();
    }
}