using System;
using System.IO;
using System.Text;

namespace TrollsGame
{
    class TrollsGen
    {
        public static bool hasEqualNeighbour(int r, int c, int[,] b)
        {
            int val = b[r, c];
            if (val == 0) return false;
            if (r > 0 && val == b[r - 1, c]) return true;
            if (r < (b.GetLength(0)-1) && val == b[r + 1, c]) return true;
            if (c > 0 && val == b[r, c - 1]) return true;
            if (c < (b.GetLength(1) - 1) && val == b[r, c + 1]) return true;
            return false;
        }

        static void Main()
        {
            Console.BufferWidth = Console.WindowWidth = 100;
            Random Generator = new Random();
            Console.Write("Please enter desired moves C [2,1000] (or press Enter for random)");
            int C = 0;
            if (!int.TryParse(Console.ReadLine(), out C) || C < 2 || C > 1000)
            {
                C = Generator.Next(1000) + 1;
            }
            Console.WriteLine("Selected moves C = " + C);

            Console.Write("Please enter desired board dimensions N x N, N [2,1000] (or press Enter for random)");
            int N = 0;
            if (!int.TryParse(Console.ReadLine(), out N) || N < 2 || N > 1000)
            {
                N = Generator.Next(1000) + 1;
            }
            Console.WriteLine("Selected dimensions N x N, N = " + N);

            Console.Write("Please enter desired height limitation, H [1,1000] (or press Enter for random)");
            int H = 0;
            if (!int.TryParse(Console.ReadLine(), out H) || N < 1 || H > 1000)
            {
                H = Generator.Next(1001);
            }
            Console.WriteLine("Selected height limitation, H = " + H);

            int[,] board = new int[N, N];
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                {
                    do
                    {
                        board[r, c] = Generator.Next(H + 1);
                    }
                    while (hasEqualNeighbour(r, c, board));
                    if (N < 100)
                    {
                        Console.Write("{0,5} ", board[r, c]);
                        if ((N - c) == 1) Console.WriteLine();
                    }
                }

            string filename = "";
            bool isValid = false;
            while (!isValid)
	        {
                Console.Write("Please enter valid filename, (.txt will be added automatically)");
                filename = Console.ReadLine();
                isValid = char.IsLetter(filename[0]);
                for (int i = 1; i < filename.Length; i++)
                {
                    isValid = isValid && (char.IsLetterOrDigit(filename[i]) || char.IsWhiteSpace(filename[i]));
                }	         
	        }
            filename = filename + ".txt";
            Console.WriteLine("Selected filename: " + filename);
            using (StreamWriter file = new StreamWriter(filename))
            {
                file.WriteLine(C);
                file.WriteLine(N);
                for (int r = 0; r < N; r++)
                    for (int c = 0; c < N; c++)
                    {
                        file.Write(board[r, c]);
                        if ((N - c) == 1) file.WriteLine();
                        else file.Write(' ');
                    }

            }
        }
    }
}
