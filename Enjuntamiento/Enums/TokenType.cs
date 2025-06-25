namespace PixelWallE
{
    public enum TokenType
    {
        // Palabras clave e instrucciones
        Spawn, Color, Size, DrawLine, DrawCircle, DrawRectangle, Fill, GoTo,

        // Funciones
        GetActualX, GetActualY, GetCanvasSize, GetColorCount,
        IsBrushColor, IsBrushSize, IsCanvasColor,

        // Tipos y valores
        Number, ColorLiteral, String, Identifier, Boolean,

        // Símbolos
        LeftParen, RightParen, LeftBracket, RightBracket,
        Comma, Arrow, Colon,

        // Operadores
        Plus, Minus, Multiply, Divide, Modulo, Power,
        Equal, NotEqual, Greater, GreaterEqual, Less, LessEqual,
        And, Or, Not,

        // Estructuras
        Label, Assignment,

        // Control
        EndOfLine, EndOfFile,

        // Comentarios
        Comment
    }

}