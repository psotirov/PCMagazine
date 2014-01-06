using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleExecutor
{
    class Bullet
    {
        const double BulletSpeed = 10;

        public Vector2 center;
        Vector2 speed;

        public Bullet(Vector2 center, Vector2 target)
        {
            this.center = center;
            this.speed = (target - center).GetNormalized() * BulletSpeed;
        }

        public Vector2 GetNextPosition()
        {
            return this.center + speed;
        }

        public void Move()
        {
            this.center += speed;
        }

        public List<Vector2> GetCurrentHitPositions()
        {
            var hitPositions = new List<Vector2>();

            for (int i = 0; i < BulletSpeed; i++)
            {
                hitPositions.Add(this.center - (this.speed * (i / BulletSpeed)));
            }

            return hitPositions;
        }
    }
}
