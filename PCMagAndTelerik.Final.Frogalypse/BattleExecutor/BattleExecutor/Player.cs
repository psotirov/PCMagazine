using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleExecutor
{
    class Player
    {
        const double MaxPlayerMoveDist = 2;

        public Circle body;
        public string name;
        public AlgorithmExecutor algorithm;
        public bool isAlive;
        public int reloadTimeRemaining;
        public List<Bullet> bullets;

        public Player(AlgorithmExecutor algorithm, Circle body)
        {
            this.algorithm = algorithm;
            this.name = algorithm.AlgorithmOwner;
            this.body = body;
            this.isAlive = true;
            this.reloadTimeRemaining = 0;
            this.bullets = new List<Bullet>();
        }

        public void Move(Vector2 target)
        {
            Vector2 moveVector = target - this.body.Center;
            double moveVectorMagn = moveVector.Magnitude();
            if (moveVectorMagn > MaxPlayerMoveDist)
            {
                moveVector.Normalize();
                this.body.Center += (moveVector * MaxPlayerMoveDist);
            }
            else
            {
                this.body.Center = target;
            }
        }
    }
}
