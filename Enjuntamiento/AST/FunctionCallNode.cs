namespace PixelWallE
{
    public class FunctionCallNode : ExpressionNode
    {
        public TokenType FunctionName { get; set; }
        public List<ExpressionNode> Arguments { get; } = new List<ExpressionNode>();
    }
}