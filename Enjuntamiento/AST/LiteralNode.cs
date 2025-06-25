namespace PixelWallE
{
    public class LiteralNode : ExpressionNode
    {
        public object? Value { get; set; }
        public TokenType ValueType { get; set; }
    }
}