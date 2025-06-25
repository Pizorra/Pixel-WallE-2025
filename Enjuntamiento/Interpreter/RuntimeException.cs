using System;
using System.Collections.Generic;



namespace PixelWallE
{

    public class RuntimeException : Exception
    {
        public int Line { get; }
        public int Position { get; }

        public RuntimeException(string message, int line, int position)
            : base(message)
        {
            Line = line;
            Position = position;
        }
    }
}