using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleExecutor
{
    class Program
    {
        static Player firstPlayer;
        static Player secondPlayer;

        static PlayerCommandEvaluator firstPlayerCommandEvaluator;
        static PlayerCommandEvaluator secondPlayerCommandEvaluator;

        static Circle puddle = new Circle(new Vector2(0, 0), 100);

        const double firstPlayerBodyX = -80;
        const double secondPlayerBodyX = 80;
        const double playerBodyRadius = 3.5;

        static void Main(string[] args)
        {
            LoadPlayers();

            string matchName = firstPlayer.algorithm.AlgorithmOwner + "-vs-" + secondPlayer.algorithm.AlgorithmOwner;

            if (System.IO.Directory.Exists(matchName))
            {
                System.IO.Directory.Delete(matchName, true);
            }
            System.IO.Directory.CreateDirectory(matchName);

            int turn = 0;
            while (firstPlayer.isAlive && secondPlayer.isAlive)
            {
                RunBattleStep();
                System.IO.File.WriteAllText(matchName + "/" + (turn).ToString().PadLeft(3, '0') + ".txt", GetBattleStateString(firstPlayer));
                Console.WriteLine(turn++);
            }

            string resultContent;
            if (firstPlayer.isAlive)
            {
                resultContent = firstPlayer.algorithm.AlgorithmOwner + " 2" + Environment.NewLine + secondPlayer.algorithm.AlgorithmOwner + " 0";
            }
            else if (secondPlayer.isAlive)
            {
                resultContent = secondPlayer.algorithm.AlgorithmOwner + " 2" + Environment.NewLine + firstPlayer.algorithm.AlgorithmOwner + " 0";
            }
            else
            {
                resultContent = secondPlayer.algorithm.AlgorithmOwner + " 1" + Environment.NewLine + firstPlayer.algorithm.AlgorithmOwner + " 1";
            }

            System.IO.File.WriteAllText(matchName + ".txt", resultContent);

            Console.WriteLine(resultContent);
        }

        private static void RunBattleStep()
        {
            MoveAndCheckBulletCollisions();

            if ((!firstPlayer.isAlive) || (!secondPlayer.isAlive))
            {
                return;
            }

            if (firstPlayer.reloadTimeRemaining > 0)
            {
                firstPlayer.reloadTimeRemaining--;
            }

            if (secondPlayer.reloadTimeRemaining > 0)
            {
                secondPlayer.reloadTimeRemaining--;
            }

            string battleStateFirstPlayer = GetBattleStateString(firstPlayer);
            string battleStateSecondPlayer = GetBattleStateString(secondPlayer);
            CommunicateWithFirstPlayer(battleStateFirstPlayer);
            CommunicateWithSecondPlayer(battleStateSecondPlayer);

            CheckPlayerCollisions();

            puddle.Radius--;
        }

        private static void CheckPlayerCollisions()
        {
            if (firstPlayer.body.Intersects(secondPlayer.body))
            {
                firstPlayer.isAlive = false;
                secondPlayer.isAlive = false;
            }

            if (!puddle.Contains(firstPlayer.body))
            {
                firstPlayer.isAlive = false;
            }

            if (!puddle.Contains(secondPlayer.body))
            {
                secondPlayer.isAlive = false;
            }
        }

        private static void MoveAndCheckBulletCollisions()
        {
            foreach (var bullet in firstPlayer.bullets)
            {
                bullet.Move();
                var bulletHitPositions = bullet.GetCurrentHitPositions();
                foreach (var hitPosition in bulletHitPositions)
                {
                    if (secondPlayer.body.Contains(hitPosition))
                    {
                        secondPlayer.isAlive = false;
                    }
                }
            }

            foreach (var bullet in secondPlayer.bullets)
            {
                bullet.Move();
                var bulletHitPositions = bullet.GetCurrentHitPositions();
                foreach (var hitPosition in bulletHitPositions)
                {
                    if (firstPlayer.body.Contains(hitPosition))
                    {
                        firstPlayer.isAlive = false;
                    }
                }
            }

            firstPlayer.bullets.RemoveAll((b) =>
            {
                return !puddle.Contains(b.center);
            });

            secondPlayer.bullets.RemoveAll((b) =>
            {
                return !puddle.Contains(b.center);
            });
        }

        private static void CommunicateWithSecondPlayer(string battleState)
        {
            //string battleState = GetBattleStateString(secondPlayer);
            DynamicTest battleStateData = new DynamicTest(battleState);

            var evaluator = new AlgorithmEvaluator(null, secondPlayerCommandEvaluator, TimeSpan.FromMilliseconds(200 + 100));

            //secondPlayer.algorithm.StartAlgorithm();
            evaluator.EvaluateWithTest(secondPlayer.algorithm, battleStateData);
            //secondPlayer.algorithm.KillAlgorithm();
        }

        private static void CommunicateWithFirstPlayer(string battleState)
        {
            //string battleState = GetBattleStateString(firstPlayer);
            DynamicTest battleStateData = new DynamicTest(battleState);

            var evaluator = new AlgorithmEvaluator(null, firstPlayerCommandEvaluator, TimeSpan.FromMilliseconds(200 + 100));

            //firstPlayer.algorithm.StartAlgorithm();
            evaluator.EvaluateWithTest(firstPlayer.algorithm, battleStateData);
            //firstPlayer.algorithm.KillAlgorithm();
        }

        private static string GetBattleStateString(Player viewingPlayer)
        {
            StringBuilder battleState = new StringBuilder();

            battleState.AppendLine(puddle.Radius.ToString());

            if (viewingPlayer == firstPlayer)
            {
                battleState.AppendLine(firstPlayer.body.Center.x + " " + firstPlayer.body.Center.y + " " + firstPlayer.reloadTimeRemaining);
                battleState.AppendLine(secondPlayer.body.Center.x + " " + secondPlayer.body.Center.y + " " + secondPlayer.reloadTimeRemaining);
            }

            if (viewingPlayer == secondPlayer)
            {
                battleState.AppendLine(secondPlayer.body.Center.x + " " + secondPlayer.body.Center.y + " " + secondPlayer.reloadTimeRemaining);
                battleState.AppendLine(firstPlayer.body.Center.x + " " + firstPlayer.body.Center.y + " " + firstPlayer.reloadTimeRemaining);
            }

            int totalBulletsCount = firstPlayer.bullets.Count + secondPlayer.bullets.Count;

            battleState.AppendLine(totalBulletsCount.ToString());

            List<Bullet> allBullets = new List<Bullet>();
            allBullets.AddRange(firstPlayer.bullets);
            allBullets.AddRange(secondPlayer.bullets);

            foreach (var bullet in allBullets)
            {
                var bulletCurrent = bullet.center;
                var bulletNext = bullet.GetNextPosition();

                battleState.AppendLine(bulletCurrent.x + " " + bulletCurrent.y + " " + bulletNext.x + " " + bulletNext.y);
            }

            return battleState.ToString();
        }

        private static void LoadPlayers()
        {
            string[] algDirs = System.IO.Directory.GetDirectories("algorithms");

            AlgorithmExecutor firstPlayerAlgorithm = new AlgorithmExecutor(algDirs[0]);
            AlgorithmExecutor secondPlayerAlgorithm = new AlgorithmExecutor(algDirs[1]);

            firstPlayer = new Player(firstPlayerAlgorithm, new Circle(new Vector2(firstPlayerBodyX, 0), playerBodyRadius));
            secondPlayer = new Player(secondPlayerAlgorithm, new Circle(new Vector2(secondPlayerBodyX, 0), playerBodyRadius));

            firstPlayerCommandEvaluator = new PlayerCommandEvaluator(firstPlayer);
            secondPlayerCommandEvaluator = new PlayerCommandEvaluator(secondPlayer);
        }
    }
}
