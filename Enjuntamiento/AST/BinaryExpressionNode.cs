namespace PixelWallE
{
    public class BinaryExpressionNode : ExpressionNode
    {
        public TokenType Operator { get; set; }
        public ExpressionNode? Left { get; set; }
        public ExpressionNode? Right { get; set; }
    }
}