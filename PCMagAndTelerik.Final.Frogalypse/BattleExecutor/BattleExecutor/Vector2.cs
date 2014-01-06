using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleExecutor
{
    struct Vector2
    {
        public double x;
        public double y;

        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(Vector2 a, double coef)
        {
            return new Vector2(a.x * coef, a.y * coef);
        }

        public double Magnitude()
        {
            return Math.Sqrt((this.x * this.x) + (this.y * this.y));
        }

        public void Normalize()
        {
            double magnitude = this.Magnitude();
            this.x /= magnitude;
            this.y /= magnitude;
        }

        public Vector2 GetNormalized()
        {
            double magnitude = this.Magnitude();
            return new Vector2(this.x / magnitude, this.y / magnitude);
        }
    }
}
