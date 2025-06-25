namespace PixelWallE
{
    public class UnaryExpressionNode : ExpressionNode
    {
        public TokenType Operator { get; set; }
        public ExpressionNode? Operand { get; set; }
    }
}