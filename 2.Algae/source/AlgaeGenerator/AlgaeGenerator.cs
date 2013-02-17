using System;
using System.Linq;
using System.IO;

namespace PCMagAndTelerik_2_Algae
{
    class AlgaeGenerator
    {
        static void Main()
        {
            Random rand = new Random();
            int inputT = 0;
            Console.Write("Please enter time T [1, 1000] or Enter for random: ");
            if (!int.TryParse(Console.ReadLine(), out inputT) || inputT < 1 || inputT > 1000)
            {
                inputT = rand.Next(100);
            }
            Console.WriteLine(inputT);

            int inputN = 0;
            Console.Write("Please enter field dimension N [3, 1000] or Enter for random: ");
            if (!int.TryParse(Console.ReadLine(), out inputN) || inputN < 3 || inputN > 1000)
            {
                inputN = rand.Next(3,1000);
            }
            Console.WriteLine(inputN);

            int inputV = 0;
            Console.Write("Please enter algae count V [3, {0}] or Enter for random: ", inputN*inputN-5);
            if (!int.TryParse(Console.ReadLine(), out inputV) || inputV < 3 || inputV > (inputN*inputN-5))
            {
                inputV = rand.Next(3, inputN*inputN-5);
            }
            Console.WriteLine(inputV);

            int inputF = 0;
            Console.Write("Please enter food count F [1, {0}] or Enter for random: ", inputN * inputN - 5);
            if (!int.TryParse(Console.ReadLine(), out inputF) || inputF < 1 || inputF > (inputN * inputN - 5))
            {
                inputF = rand.Next(1, inputN * inputN - 5);
            }
            Console.WriteLine(inputF);

            char[,] board = new char[inputN, inputN];
            for (int i = 0; i < inputF; i++)
            {
                int r = 0;
                int c = 0;
                do
                {
                    r = rand.Next(inputN);
                    c = rand.Next(inputN);
                } while (board[r,c] != 0);
                board[r,c] = 'F';
            }

            using(StreamWriter file = new StreamWriter("test.txt"))
            {
                file.WriteLine(inputT);
                Console.WriteLine(inputT);
                file.WriteLine(inputV);
                Console.WriteLine(inputV);
                file.WriteLine(inputN);
                Console.WriteLine(inputN);
                for (int i = 0; i < inputN; i++)
                {
                    for (int j = 0; j < inputN; j++)
                    {
                        if (board[i, j] != 0)
                        {
                            file.Write('F');
                            Console.Write('F');
                        }
                        else
                        {
                            file.Write('0');
                            Console.Write('0');
                        }                        
                    }
                    file.WriteLine();
                    Console.WriteLine();
                }
            }
            Console.WriteLine("Please press Enter");
            Console.ReadLine();
        }
    }
}
