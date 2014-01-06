using System;

namespace Frogalypse.Algorithm
{
    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Position(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public Position Scale(double factor)
        {
            return new Position(this.X * factor, this.Y * factor);
        }

        public Position Add(Position vector)
        {
            return new Position(this.X + vector.X, this.Y + vector.Y);
        }
    }
}
