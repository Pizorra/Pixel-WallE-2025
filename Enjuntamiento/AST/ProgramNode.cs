namespace PixelWallE
{
    public class ProgramNode : ASTNode
    {
        public List<ASTNode> Statements { get; } = new List<ASTNode>();
    }
}