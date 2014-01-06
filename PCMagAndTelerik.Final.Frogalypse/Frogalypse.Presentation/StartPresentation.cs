using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Frogalypse.Algorithm;

namespace Frogalypse.Presentation
{
    class StartPresentation
    {
        static bool[] algoAlive = new bool[] {true, true};
        static int bothAlgosCounter = 200;
        static Frog[] frogs = new Frog[2];
        static List<Bullet>[] bullets = new List<Bullet>[2];
        static StringBuilder output = new StringBuilder();

        static void Main(string[] args)
        {
            Console.Write("Combatting both algorithms:\n\n...");

            // initiallize pool 
            string sendtext =
                "100" + Environment.NewLine +
                "-80 0 0" + Environment.NewLine +
                "80 0 0" + Environment.NewLine +
                "0" + Environment.NewLine;
            frogs[0] = new Frog(-80, 0, 0);
            frogs[1] = new Frog(80, 0, 0);
            bullets[0] = new List<Bullet>();
            bullets[1] = new List<Bullet>();
            Frog.PoolRadus = 100;

            // first algorithm
            string algo1file = @"..\..\algo1.exe";
            string algo2file = @"..\..\algo2.exe";
            string jsfile = @"..\..\data.js";
            string htmlfile = @"..\..\Presentation.html";

            // main loop of both algos
            while (algoAlive[0] && algoAlive[1])
            {
                string getResponse = QueryAlgo(algo1file, sendtext);
                SaveMove(getResponse);

                sendtext = GetInput();
                getResponse = QueryAlgo(algo2file, sendtext);
                SaveMove(getResponse);

                EvaluatePool();
                TakeSnapshot();
                Console.Write(".");
                sendtext = GetInput();
            }

            SaveJSON(jsfile);
            Console.WriteLine("\nDone!\n\nStarting default browser for presentation...");

            Process browser = new Process();
            browser.StartInfo.FileName = htmlfile;
            browser.Start();
            // browser.WaitForExit();
            // browser.Close();
        }

        static void SaveMove( string text = "")
        {
            text = text.Replace(Environment.NewLine, " ");
            string[] input = text.Split(' ');

            // shoot first from current position if it can
            if (input.Length > 3 && input[3] == "shoot" && frogs[bothAlgosCounter % 2].TimeToReload == 0)
            {
                Position target = new Position(double.Parse(input[4]), double.Parse(input[5]));
                Position vector = Calc.GetVector(frogs[bothAlgosCounter % 2].Pos, target);
                // puts bullet ar pair of current position and single vector for the next move
                bullets[bothAlgosCounter % 2].Add(
                    new Bullet(frogs[bothAlgosCounter % 2].Pos.X, frogs[bothAlgosCounter % 2].Pos.Y, vector.X, vector.Y));
                // sets reload waiting time
                frogs[bothAlgosCounter % 2].TimeToReload = 8;
            }

            // then moves to new position
            if (input[0] == "move")
            {
                // gets the requested new position and if it is above distance of MaxJumpDistance (2.0)
                // calculates the vector, scales it and add it to the current position
                Position next = new Position(double.Parse(input[1]), double.Parse(input[2]));
                if (Calc.GetDistance(frogs[bothAlgosCounter % 2].Pos, next) > Frog.MaxJumpDistance)
                {
                    frogs[bothAlgosCounter % 2].Pos = 
                        frogs[bothAlgosCounter % 2].Pos.Add(Calc.GetVector(frogs[bothAlgosCounter % 2].Pos, next).Scale(2.0));
                }
                else
                {
                    frogs[bothAlgosCounter % 2].Pos = next;
                }
            }
        }

        static string GetInput()
        {
            string result = (bothAlgosCounter / 2).ToString() + Environment.NewLine;
            Frog enemy = frogs[bothAlgosCounter % 2];
            string enemyData = enemy.Pos.X.ToString() + " " + enemy.Pos.Y + " " + enemy.TimeToReload + Environment.NewLine;
            enemyData += bullets[bothAlgosCounter % 2].Count + Environment.NewLine;
            foreach (var bullet in bullets[bothAlgosCounter % 2])
            {
                // finds target position - pos + vector * speed (10)
                Position target = bullet.Pos.Add(bullet.Target.Scale(10));
                enemyData += bullet.Pos.X + " " + bullet.Pos.Y + " " + target.X + " " + target.Y + Environment.NewLine;
            }

            bothAlgosCounter--; // goes to next frog
            Frog current = frogs[bothAlgosCounter % 2];
            result += current.Pos.X.ToString() + " " + current.Pos.Y + " " + current.TimeToReload + Environment.NewLine;
            result += enemyData;
            return result;
        }

        static void EvaluatePool()
        {
            Position poolCenter = new Position(0, 0);
            // checks for collision between frogs - both are dead
            if (Calc.GetDistance(frogs[0].Pos, frogs[1].Pos) < 2 * Frog.FrogRadius)
            {
                algoAlive[0] = false;
                algoAlive[1] = false;
                return;
            }
            // checks for collision between each frog and pool border - each is dead
            for (int i = 0; i < frogs.Length; i++)
            {
                if (Calc.GetDistance(frogs[i].Pos, poolCenter) + Frog.FrogRadius > Frog.PoolRadus)
                {
                    algoAlive[i] = false;
                    return;
                }                
            }
           
            // checks for collision with enemy bullet
            for (int i = 0; i < frogs.Length; i++)
            {
                for (int bul = 0; bul < bullets[i].Count; bul++)
                {
                    for (int j = 0; j < Bullet.BulletSpeed; j++)
                    {
                        if (Calc.GetDistance(bullets[i][bul].Pos, frogs[frogs.Length - i - 1].Pos) < Frog.FrogRadius)
                        {
                            algoAlive[frogs.Length - i - 1] = false;
                        }

                        // adds single target vector to current bullet position
                        bullets[i][bul].Pos = bullets[i][bul].Pos.Add(bullets[i][bul].Target);

                        // checks for out of pool condition
                        if (Calc.GetDistance(bullets[i][bul].Pos, poolCenter) > Frog.PoolRadus)
                        {
                            bullets[i].RemoveAt(bul);
                            break;
                        }
                    }
                }
            }

            // update pool radius
            Frog.PoolRadus--;
        }

        static string QueryAlgo(string file, string input)
        {
            Process algo1 = new Process();
            algo1.StartInfo.FileName = file;
            algo1.StartInfo.UseShellExecute = false;
            algo1.StartInfo.RedirectStandardInput = true;
            algo1.StartInfo.RedirectStandardOutput = true;
            algo1.Start();

            StreamWriter outputToAlgo1 = algo1.StandardInput;
            StreamReader inputFromAlgo1 = algo1.StandardOutput;

            outputToAlgo1.WriteLine(input);
            string response = inputFromAlgo1.ReadToEnd();

            inputFromAlgo1.Close();
            outputToAlgo1.Close();
            algo1.Close();

            return response;
        }

        static void TakeSnapshot()
        {
            output.AppendFormat("{{ PoolRadius: {0}, Frogs: [", Frog.PoolRadus );
            for (int i = 0; i < frogs.Length; i++)
            {
                output.AppendFormat("{{ PosX: {0}, PosY: {1}, Bullets: [", frogs[i].Pos.X, frogs[i].Pos.Y);
                foreach (var bullet in bullets[i])
                {
                    Position target = bullet.Pos.Add(bullet.Target.Scale(10));
                    output.AppendFormat("{{ PosX: {0}, PosY: {1}, TargetX: {2}, TargetY: {3} }},",
                        bullet.Pos.X, bullet.Pos.Y, target.X, target.Y);                    
                }

                output.Append("] }, ");
            }

            output.AppendLine("] },");

            //Console.WriteLine(output.ToString());
        }

        static void SaveJSON(string file)
        {
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.WriteLine("var data = [" + output + "];");
                sw.WriteLine("var results = [ {0}, {1} ];", (algoAlive[0]?1:0), (algoAlive[1]?1:0));  
            }
        }
    }
}
