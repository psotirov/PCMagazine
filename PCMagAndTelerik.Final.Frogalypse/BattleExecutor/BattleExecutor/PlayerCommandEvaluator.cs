using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleExecutor
{
    class PlayerCommandEvaluator : IAnswerEvaluator
    {
        Player player;

        const string MoveCommandName = "move";
        const string ShootCommandName = "shoot";
        const int reloadTime = 7;

        public PlayerCommandEvaluator(Player player)
        {
            this.player = player;
        }

        public int EvaluateAnswer(string answer, string question, string expected)
        {
            bool moveCommandUsed = false;
            bool shootCommandUsed = false;
            var lastBodyCenter = player.body.Center;
            try
            {
                string[] answerLines = answer.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in answerLines)
                {
                    string[] splitLine = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitLine[0] == MoveCommandName && !moveCommandUsed)
                    {
                        double moveX = double.Parse(splitLine[1]),
                            moveY = double.Parse(splitLine[2]);

                        this.player.Move(new Vector2(moveX, moveY));
                    }

                    else if (splitLine[0] == ShootCommandName && !shootCommandUsed && this.player.reloadTimeRemaining == 0)
                    {
                        double shotX = double.Parse(splitLine[1]),
                            shotY = double.Parse(splitLine[2]);

                        var targetPosition = new Vector2(shotX, shotY);

                        if (targetPosition.x != lastBodyCenter.x || targetPosition.y != lastBodyCenter.y)
                        {
                            this.player.bullets.Add(new Bullet(lastBodyCenter, targetPosition));
                            this.player.reloadTimeRemaining = reloadTime + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return 0;
        }

        public void UseLogWriter(System.IO.TextWriter writer)
        {

        }
    }
}
