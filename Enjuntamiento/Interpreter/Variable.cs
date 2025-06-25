namespace PixelWallE
{
    public class Variable
    {
        public int NumericValue { get; set; }
        public bool BoolValue { get; set; }
        public string StringValue { get; set; }
        public bool IsBoolean { get; set; }
        public TokenType ValueType { get; set; }
    }
}