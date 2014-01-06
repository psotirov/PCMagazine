using System;

namespace Frogalypse.Algorithm
{
    public class Frog
    {
        public const double FrogRadius = 3.5;
        public const double MaxJumpDistance = 2.0;
        const double MinimalDistanceToBorder = 3.9;
        const double MinJumpDistance = 1.22;

        private static Random generator;
        public static double PoolRadus;
        public Position Pos { get; set; }
        public Position NextMovePos { get; set; }

        public int TimeToReload { get; set; }
        public bool CanShoot { get; set; }
        public bool IsShooting { get; set; }
        public Position ShootTargetPos { get; set; }

        public Frog(double xpos, double ypos, int reload)
        {
            generator = new Random();
            this.Pos = new Position(xpos, ypos);
            // initially the frog will not move
            this.NextMovePos = new Position(xpos, ypos);

            this.TimeToReload = reload;
            this.CanShoot = (reload == 0);
            // initially the frog will not shoot
            this.IsShooting = false;
            // and set the taget to pool center
            this.ShootTargetPos = new Position(0,0);
        }

        public Position GetFinalPosition()
        {
            return new Position(this.NextMovePos.X, this.NextMovePos.Y);
        }

        public bool GetShoot(out Position target)
        {
            bool shoot = (this.IsShooting && this.CanShoot);
            target = new Position(this.ShootTargetPos.X, this.ShootTargetPos.Y); // copy
            return shoot;
        }

        public void TakeDecision(Frog enemy, Bullet[] bullets)
        {
            bool frogMoved = false;

            // Take care of the incoiming bullets
            if (!frogMoved)
            {
                frogMoved = this.RunFromBullets(bullets);
            }

            // Watch out for the collapsing pool
            if (!frogMoved)
            {
                frogMoved = this.RunFromPoolBorder();
            }

            // Watch out of the enemy frog
            if (!frogMoved)
            {
                frogMoved = this.RunFromEnemy(enemy);

            }

            // Aggresive mode - predict enemy move and shoot
            this.ShootAtWill(enemy);
        }

        public bool RunFromPoolBorder()
        {
            bool moved = false;
            Position center = new Position(0, 0);
            Position toCheck = this.Pos;
            double distanceToBorder = PoolRadus - (Calc.GetDistance(toCheck, center) + FrogRadius); 
            if (distanceToBorder < MinimalDistanceToBorder)
            {
                // move to the center immediately with max speed
                this.NextMovePos = this.Pos.Add(Calc.GetVector(toCheck, center).Scale(MaxJumpDistance));
                moved = true;
            }

            return moved;
        }

        public bool RunFromBullets(Bullet[] bullets)
        {
            foreach (var bullet in bullets)
            {
                // get bullet distance to the front of the frog
                double distanceToBullet = Calc.GetDistance(this.Pos, bullet.Pos) - Frog.FrogRadius; 
 
                // check if my frog is in range (in 2 moves bullet can reach the frog, but we can run away in these 2 moves)
                if (distanceToBullet <= 2 * Bullet.BulletSpeed)
                {
                    // get bullet direction
                    Position bulletVector = Calc.GetVector(bullet.Pos, bullet.Target);

                    // detect possible collision - check the bullet movement vector scaled by distance + frog diameter
                    for (double i = 0; i <= 2 * Frog.FrogRadius; i += Bullet.BulletLandingDistance)
                    {
                        Position bulletPos = bullet.Pos.Add(bulletVector.Scale(distanceToBullet + i));
                        if (Calc.HasImpact(this.Pos, Frog.FrogRadius, bulletPos))
                        {
                            // we have found a collision - run away immediately
                            // go to opposite side, i.e. rotate bullet vector to 90 degrees clockwise or counter-clockwise and jump
                            //TODO: select appropriate vector
                            //if (true) 
                            //{
                                this.NextMovePos = this.Pos.Add(Calc.RotateVectorCounterClockwise(bulletVector).Scale(Frog.MaxJumpDistance));
                            //}
                            //else
                            //{
                            //    this.NextMovePos = this.Pos.Add(Calc.RotateVectorClockwise(bulletVector).Scale(Frog.MaxJumpDistance));
                            //}
                            return true; // return with movement indication
                        }
                    }
                }
            }

            return false; // there is no dangerous bullets
        }

        public bool RunFromEnemy(Frog enemy)
        {
            // try to keep distance to enemy as much as possible
            // move randomly to the left or to the right of the vector between frogs
            
            // get enemy direction
            Position enemyVector = Calc.GetVector(this.Pos, enemy.Pos);

            // hardcoded decision table with random selector
            Position[] moves = new Position[12];
            moves[0] = Calc.GetVector(this.Pos, new Position(0, 0)); // run to center
            moves[1] = Calc.RotateVectorClockwise(enemyVector); // run clockwise to the enemy position
            moves[2] = Calc.RotateVectorCounterClockwise(enemyVector); // run counter-clockwise to the enemy position
            moves[3] = moves[0]; // run to center
            moves[4] = moves[0]; // run to center
            moves[5] = moves[1]; // run counter-clockwise to the enemy position
            moves[6] = moves[1]; // run counter-clockwise to the enemy position
            moves[7] = moves[0]; // run to center
            moves[8] = moves[2]; // run counter-clockwise to the enemy position
            moves[9] = moves[1]; // run counter-clockwise to the enemy position
            moves[10] = enemyVector; // run forward to the enemy
            moves[11] = new Position(0,0); // stay at current position
            
            double distanceToEnemy = Calc.GetDistance(this.Pos, enemy.Pos);
            double scaleFactor = generator.NextDouble() * (Frog.MaxJumpDistance - Frog.MinJumpDistance) + Frog.MinJumpDistance;

            // check if frogs are in range (in 2 moves bullet can reach the frog, but we can run away in these 2 moves)
            bool closeToEnemy = (distanceToEnemy < 2 * Bullet.BulletSpeed);

            // and if yes - exclude last two choices - stay and move forward
            int chooseMove = generator.Next(closeToEnemy ? moves.Length - 2 : moves.Length);
            this.NextMovePos = this.Pos.Add(moves[chooseMove].Scale(scaleFactor));

            if (closeToEnemy)
            {
                ShootAtEnemy(enemy);
            }

            return (chooseMove != 3); // if stays then no move has been selected
        }

        public void ShootAtWill(Frog enemy)
        {
            // predict enemy move using the same algorithm
            // shoots at enemy with a chance of 1/10 
            if (generator.Next(10) == 5)
            {
                ShootAtEnemy(enemy);
            }

            // calculate 
        }

        public void ShootAtEnemy(Frog enemy)
        {
            // take a shot to the enemy's center
            this.ShootTargetPos = new Position(enemy.Pos.X, enemy.Pos.Y);
            this.IsShooting = true;
        }
    }
}
