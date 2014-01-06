using System;

namespace Frogalypse.Algorithm
{
    public static class Calc
    {
        public static double GetDistance(Position source, Position target)
        {
            return Math.Sqrt((source.X - target.X) * (source.X - target.X) + (source.Y - target.Y) * (source.Y - target.Y));
        }

        public static Position GetVector(Position source, Position target)
        {
            double distance = Calc.GetDistance(source, target);
            double deltaX = (target.X - source.X) / distance;
            double deltaY = (target.Y - source.Y) / distance;
            return new Position(deltaX,deltaY); 
        }

        public static Position RotateVectorClockwise(Position source)
        {
            // (x,y) => (-y, x)
            return new Position(-source.Y, source.X);
        }

        public static Position RotateVectorCounterClockwise(Position source)
        {
            // (x,y) => (y, -x)
            return new Position(source.Y, -source.X);
        }

        public static bool HasImpact(Position source, double radius, Position target)
        {
            return (GetDistance(source, target) <= radius);
        }
    }
}
