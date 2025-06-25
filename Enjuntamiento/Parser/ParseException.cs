using System;
using System.Collections.Generic;

namespace PixelWallE
{
    public class ParseException : Exception
    {
        public int Line { get; }
        public int Position { get; }

        public ParseException(string message, int line, int position)
            : base($"{message} at line {line}, position {position}")
        {
            Line = line;
            Position = position;
        }
    }
}