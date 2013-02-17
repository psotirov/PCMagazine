using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TrollsGame
{
    class Tower
    {
        public const int maxHeight = 1000;
        int Height;

        public Tower() //default constructor
        {
            Height = 0;
        }

        public Tower(int h) // constructor that sets initial height
        {
            if (h > 0 && h <= maxHeight) Height = h;
            else Height = 0;
        }

        public static Tower operator ++(Tower t) // overloads increment operator 
        {
            t.Height++;
            return t;
        }

        public static Tower operator --(Tower t) // overloads decrement operator 
        {
            t.Height--;
            return t;
        }

        public bool isEmpty() // checks if tower has no height
        {
            return (Height == 0);
        }

        public bool isFull() // checks if tower has "maxHeight"
        {
            return (Height >= maxHeight);
        }

        public void Destroy() // sets the tower to have no height
        {
            Height = 0;
        }

        public void Set(int h) // sets the exact height of tower
        {
            Height = h;
        }

        public int Get() // returns the exact height of tower
        {
            return Height;
        }
    }

    struct Move
    {
        public int type;
        public int row;
        public int column;
    }

    class Playfield
    {
        Tower[,] Board;
        public int Score { get; set; }
        Stack<Move> OldMoves = new Stack<Move>();

        public Playfield() //default constructor - makes 2x2 towers playfield
        {
            Board = new Tower[2, 2];
            Score = 0;
        }

        public Playfield(int n) //explicit dimension constructor - makes nxn towers playfield
        {
            Board = new Tower[n, n];
            Score = 0;
        }

        public int Put(int r, int c)
        {
            Board[r, c]++; // increment the indexed tower
            Score++; // increment the score
            Move toPush = new Move(); //puts the move into the stack
            toPush.type = 1; //Put
            toPush.row = r; //Row
            toPush.column = c; //Col
            OldMoves.Push(toPush);
            Score -= CheckNeighbours(r, c); // checks for equal neighbour towers and returns the quantity of destroyed pieces
            return Score;
        }

        public int Take(int r, int c)
        {
            Board[r, c]--; // decrement the indexed tower
            Score--; // decrement the score
            Move toPush = new Move(); //puts the move into the stack
            toPush.type = 0; //Take
            toPush.row = r; //Row
            toPush.column = c; //Col
            OldMoves.Push(toPush);
            Score -= CheckNeighbours(r, c); // checks for equal neighbour towers and returns the quantity of destroyed pieces
            return Score;
        }

        private int CheckNeighbours(int r, int c) //internal function in help of Put and Take
        {
            int result = 0;
            int currentHeight = Board[r, c].Get();
            if (currentHeight == 0) return 0; // no height in the current cell, no need to check neighbours
            Move toPush = new Move(); // saving board change into the stack


            if (r > 0 && Board[r - 1, c].Get() == currentHeight) // checks upper element (if any)
            {
                Board[r - 1, c].Destroy();
                result += currentHeight;
                toPush.type = -currentHeight; //Negative type means height of a tower
                toPush.row = r - 1; //Row
                toPush.column = c; //Col
                OldMoves.Push(toPush);
            }
            if (r < (Board.GetLength(0) - 1) && Board[r + 1, c].Get() == currentHeight) // checks lower element (if any)
            {
                Board[r + 1, c].Destroy();
                result += currentHeight;
                toPush.type = -currentHeight; //Negative type means height of a tower
                toPush.row = r + 1; //Row
                toPush.column = c; //Col
                OldMoves.Push(toPush);
            }

            if (c > 0 && Board[r, c - 1].Get() == currentHeight) // checks left element (if any)
            {
                Board[r, c - 1].Destroy();
                result += currentHeight;
                toPush.type = -currentHeight; //Negative type means height of a tower
                toPush.row = r; //Row
                toPush.column = c - 1; //Col
                OldMoves.Push(toPush);
            }
            if (c < (Board.GetLength(1) - 1) && Board[r, c + 1].Get() == currentHeight) // checks right element (if any)
            {
                Board[r, c + 1].Destroy();
                result += currentHeight;
                toPush.type = -currentHeight; //Negative type means height of a tower
                toPush.row = r; //Row
                toPush.column = c + 1; //Col
                OldMoves.Push(toPush);
            }

            if (result > 0) // if there is some destroy operation the current cell also has to be destroyed
            {
                Board[r, c].Destroy();
                result += currentHeight;
                toPush.type = -currentHeight; //Negative type means height of a tower
                toPush.row = r; //Row
                toPush.column = c; //Col
                OldMoves.Push(toPush);
            }

            return result;
        }

        public int Get(int r, int c)
        {
            return Board[r, c].Get(); // return the height of indexed tower
        }

        public void Set(int r, int c, int h)
        {
            if (Board[r, c] == null) // the element is not initialized
            {
                Board[r, c] = new Tower(h); // invokes explicit value constructor
                Score += h; // and updates the Score
            }
            else
            {
                int currentHeight = Board[r, c].Get(); // takes current height of indexed tower
                Board[r, c].Set(h); // sets the new height of indexed tower
                Score += Board[r, c].Get() - currentHeight; // updates the actual score
            }
        }

        public void Resume()
        {
            Move toPop = new Move();
            while (true)
            {
                toPop = OldMoves.Pop();
                if (toPop.type == 0) // if the stacked move was TAKE, we must perform PUT operation
                {
                    Set(toPop.row, toPop.column, Get(toPop.row, toPop.column) + 1); // increases the height by 1
                    return; // out of the method
                }
                if (toPop.type == 1) //if the stacked move is PUT, we must perform TAKE operation
                {
                    Set(toPop.row, toPop.column, Get(toPop.row, toPop.column) - 1); // if could be done, decreases the height by 1
                    return; // out of the method
                }
                // otherwise loops until take or put method is found
                Set(toPop.row, toPop.column, -toPop.type); // sets the column height with inverted sign
            }
        }

        public int NeighboursDiff(int r, int c) // function that checks the minimal difference between cell and its neighbours
        {
            int difference = int.MaxValue; // the biggest possible difference
            int currentHeight = Board[r, c].Get();

            if (r > 0) // checks upper element (if any)
            {
                int h = Board[r - 1, c].Get(); // takes neighbour height
                if ((h - currentHeight) > 0) h = h - currentHeight;
                else h = currentHeight - h;
                if (h < difference) difference = h; // selects min |cell_hegiht - neighbour_height|
            }

            if (r < (Board.GetLength(0) - 1)) // checks lower element (if any)
            {
                int h = Board[r + 1, c].Get(); // takes neighbour height
                if ((h - currentHeight) > 0) h = h - currentHeight;
                else h = currentHeight - h;
                if (h < difference) difference = h; // selects min |cell_hegiht - neighbour_height|
            }

            if (c > 0) // checks left element (if any)
            {
                int h = Board[r, c - 1].Get(); // takes neighbour height
                if ((h - currentHeight) > 0) h = h - currentHeight;
                else h = currentHeight - h;
                if (h < difference) difference = h; // selects min |cell_hegiht - neighbour_height|
            }
            if (c < (Board.GetLength(1) - 1)) // checks right element (if any)
            {
                int h = Board[r, c + 1].Get(); // takes neighbour height
                if ((h - currentHeight) > 0) h = h - currentHeight;
                else h = currentHeight - h;
                if (h < difference) difference = h; // selects min |cell_hegiht - neighbour_height|
            }

            return difference;
        }
    }

    class TrollsGameCalculator
    {


        static void Main()
        {
            string firstLine = Console.ReadLine(); // reads the first line from console

            StreamReader file = null; // since there is option to read data from file, opens empty StreamReader  
            bool hasFileData = false; // by default the data should be read from console
            if (firstLine[0] == '@') // but if the first character is "@" (non digit), that means the content is filename (and path)
            {
                hasFileData = true; // then all input data should be read from the file
                firstLine = firstLine.Replace('@', ' ').Trim(); // removes "@" and any leading/trailing spaces
                file = new StreamReader(firstLine); // opens the file for reading
            }

            int C;
            int N;
            if (hasFileData) // reads from file
            {
                C = int.Parse(file.ReadLine());
                N = int.Parse(file.ReadLine());
            }
            else //reads from Console
            {
                C = int.Parse(firstLine);
                N = int.Parse(Console.ReadLine());
            }

            if (N == 1) // marginal case, one dimensional playfield
            {
                int height = int.Parse(Console.ReadLine()); // reads the single tower
                for (int i = 0; i < C; i++)
                {
                    Console.WriteLine("take 0 0"); // for a single tower playfiled only C times "take" move is allowed
                }
                return; // then program finishes
            }

            Playfield theBoard = new Playfield(N); // creates empty playfield
            for (int i = 0; i < N; i++) // iterates through rows
            {
                string[] Row;
                if (hasFileData) Row = file.ReadLine().Split(' '); // reads each line from file and separates it to array of string values 
                else Row = Console.ReadLine().Split(' '); // reads each line from console and separates it to array of string values 
                for (int j = 0; j < Row.Length; j++) // iterates through cells in current row
                {
                    theBoard.Set(i, j, int.Parse(Row[j])); // and pumps up the Playfield
                }
            }

            if (hasFileData) // if the data source is file 
            {
                file.Close(); // closes that file
                file.Dispose(); // releases all used resources 
            }
            int thsIterations = C * N * N / 1000; //calculates some estimation of the number of interations required
            // this should be used to select proper algorythm in order to fit into the 3 sec. time limit

            // Choosing algorythm depending on board and moves complexity in order to fit within 3 seconds calculation interval
            if (thsIterations < 6000) // for small and medium range a complex algorithm is selected
            {
                Move bestMove = new Move();
                //FULL WALKTROUGH - on each level (move's depth) the best result is selected
                for (int depth = 0; depth < C; depth++) // iterates through possible moves
                {
                    int maxRank = int.MinValue; // the resultng rank, initially the minimal possible value
                    for (int r = 0; r < N; r++) // iterates through possible rows
                        for (int c = 0; c < N; c++) // iterates through possible colunms
                            for (int m = 0; m < 2; m++) // iterates through possible type of moves
                            {
                                int diff = theBoard.NeighboursDiff(bestMove.row, bestMove.column) - theBoard.NeighboursDiff(r, c); // calculates if the neighbour difference of best move and current
                                int rank = theBoard.Score; // gets current score of the board
                                if (m == 0 && theBoard.Get(r, c) == 0) m = 1; // checks for invalid TAKE move, goes directly to PUT

                                if (m == 0) rank = rank - theBoard.Take(r, c); // take = 0 and calculates the rank of the move
                                else rank = rank - theBoard.Put(r, c); // put = 1 and calculates the rank of the move

                                // SELECTION OF THE BEST MOVE
                                if (rank > maxRank || // 1.
                                        (rank == maxRank && diff > 0) || // 2. and 3. 
                                            (rank == maxRank && diff == 0 && theBoard.Get(r, c) >= theBoard.Get(bestMove.row, bestMove.column))) // 2. and 4.
                                // a better move has been found if one of the following three conditons has been reached:
                                // 1. the current rank is better than the previous one (or)
                                // 2. the current rank is equal to previous one, but
                                //      2.1. the difference with some neighbours is less, (or) 
                                // 3. the current rank and the differnece are equal to previous one, but
                                //      3.1. but current height is greater 
                                {
                                    maxRank = rank; // assigns better rank of the move
                                    bestMove.type = m; // take = 0, put = 1
                                    bestMove.row = r; // row
                                    bestMove.column = c; // column
                                }
                                theBoard.Resume(); // returns to previous move
                            }
                    // now makes the best move over all iterations of current depth 
                    if (bestMove.type == 0)
                    {
                        theBoard.Take(bestMove.row, bestMove.column); // take = 0
                        Console.Write("take ");
                    }
                    else
                    {
                        theBoard.Put(bestMove.row, bestMove.column); // put = 1
                        Console.Write("put ");
                    }
                    Console.WriteLine("{0} {1}", bestMove.row, bestMove.column);
                }
            }

            else // for most complex boards only one iteration is possible (or half! or less!!)
            {
                // creates a pair of arrays for moves and their ranks
                Move[] moves = new Move[C];
                int[] ranks = new int[C];

                int rowFactor = 1; // normally all cells in the board are reached
                if (thsIterations > 300000) rowFactor = 2 + (thsIterations > 650000 ? 1 : 0);
                // unfortunately in order to fit into the time limit, for most complex boards only a half/third of them could be reached

                //SINGLE WALKTROUGH - on each iteration the result is put into the sorted array of ranks
                for (int r = 0; r < N; r = r + rowFactor) // iterates through possible rows (or half of them)
                    for (int c = 0; c < N; c++) // iterates through possible colunms
                        for (int m = 0; m < 2; m++) // iterates through possible type of moves
                        {
                            int rank = theBoard.Score; // gets current score of the board
                            if (m == 0 && theBoard.Get(r, c) > 0) rank = rank - theBoard.Take(r, c); // take = 0 and calculates the rank of the move
                            else
                            {
                                rank = rank - theBoard.Put(r, c);
                                m = 1; // put = 1 and calculates the rank of the move
                            }
                            int i = 0;
                            while (i < C && rank <= ranks[i]) i++; // looks where the current rank is suitable to place

                            if (i < C) // only if the place has found
                            {
                                if (moves[i].Equals(null)) // if the list is not full makes new item
                                    moves[i] = new Move();
                                else //and finally we found the place to insert a new better move
                                    for (int k = C - 1; k > i; k--)
                                    {
                                        moves[k] = moves[k - 1];
                                        ranks[k] = ranks[k - 1];
                                    }
                                moves[i].type = m;
                                moves[i].row = r;
                                moves[i].column = c;
                                ranks[i] = rank;
                            }
                            theBoard.Resume(); // returns to previous move
                        }

                // prints first C best ranked moves
                for (int i = 0; i < C; i++)
                // iterates through alreeady sorted best moves
                {
                    if (moves[i].type == 0)
                    {
                        theBoard.Take(moves[i].row, moves[i].column); // take = 0
                        Console.Write("take ");
                    }
                    else
                    {
                        theBoard.Put(moves[i].row, moves[i].column); // put = 1
                        Console.Write("put ");
                    }
                    Console.WriteLine("{0} {1}", moves[i].row, moves[i].column);
                }
            }
        }
    }
}
