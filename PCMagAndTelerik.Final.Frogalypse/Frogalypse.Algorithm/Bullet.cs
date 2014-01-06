using System;

namespace Frogalypse.Algorithm
{
    public class Bullet
    {
        public const double BulletSpeed = 10.0;
        public const double BulletLandingDistance = 1.0;

        public Position Pos { get; set; }
        public Position Target { get; set; }

        public Bullet(double xpos, double ypos, double xtarget, double ytarget)
        {
            this.Pos = new Position(xpos, ypos);
            this.Target = new Position(xtarget, ytarget);
        }
    }
}
