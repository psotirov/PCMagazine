using System;
using System.IO;

namespace Frogalypse.Algorithm
{
    public class Frogalipse
    {
        static Frog MyFrog;
        static Frog EnemyFrog;
        static Bullet[] Bullets;

        static void Main(string[] args)
        {
            //Console.SetIn(new StreamReader(@"..\..\input.txt"));
            GetInput();
            MyFrog.TakeDecision(EnemyFrog, Bullets);
            SendOutput();          
        }

        static void GetInput()
        {
            // first line - current pool radius
            string[] radiusLine = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            double radius = double.Parse(radiusLine[0]);
            Frog.PoolRadus = radius;

            // second line - my frog: <x position> <y position> <time to reload gun if shooted previously >
            string[] frogLine = Console.ReadLine().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            MyFrog = new Frog(double.Parse(frogLine[0]), double.Parse(frogLine[1]), int.Parse(frogLine[2]));

            // third line - enemy frog: <x position> <y position> <time to reload gun if shooted previously >
            frogLine = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            EnemyFrog = new Frog(double.Parse(frogLine[0]), double.Parse(frogLine[1]), int.Parse(frogLine[2]));

            // fourth line - number of bullets shooted
            string[] bulletsCountLine = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int bulletsCount = int.Parse(bulletsCountLine[0]);
            Bullets = new Bullet[bulletsCount];

            // next bullets count line - bullets coordinates and target position for the next move
            for (int i = 0; i < bulletsCount; i++)
            {
                string[] bulletsline = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Bullets[i] = new Bullet(double.Parse(bulletsline[0]), double.Parse(bulletsline[1]),
                    double.Parse(bulletsline[2]), double.Parse(bulletsline[3]));                
            }
        }

        static void SendOutput()
        {
            Position target = MyFrog.GetFinalPosition();
            // send move
            Console.WriteLine("move {0} {1}", target.X, target.Y);

            // send shoot
            if (MyFrog.GetShoot(out target))
            {
                Console.WriteLine("shoot {0} {1}", target.X, target.Y);
            }
            else
            {
                Console.WriteLine();
            }
        }
    }
}
