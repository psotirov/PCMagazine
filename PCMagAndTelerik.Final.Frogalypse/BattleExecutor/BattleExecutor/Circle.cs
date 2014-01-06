using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleExecutor
{
    class Circle
    {

        public Vector2 TopLeft
        {
            get
            {
                return this.Center - new Vector2(this.Radius, this.Radius);
            }
        }
        public Vector2 Center { get; set; }

        public double Radius { get; set; }

        public Circle(Vector2 center, double radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        public bool Contains(Vector2 point)
        {
            return (this.Center - point).Magnitude() <= this.Radius;
        }

        public bool Contains(Circle other)
        {
            return ((this.Center - other.Center).Magnitude() + other.Radius) < this.Radius;
        }

        public bool Intersects(Circle other)
        {
            return (this.Center - other.Center).Magnitude() <= this.Radius + other.Radius;
        }
    }
}
