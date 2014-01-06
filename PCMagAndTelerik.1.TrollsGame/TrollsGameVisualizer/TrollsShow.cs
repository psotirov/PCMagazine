using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

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

        public bool PrintBoard(string filename, int index, int initScore, int rank)
        {
            StreamWriter outFile = null; // since the output is file, opens empty StreamWriter
 
            string next = "#"; // initially there is no next move
            if (index < 0) index = -index; // cheat to inform the method this will be the last file
            else next = filename + "." + (index + 1) + ".html"; // previous move filename
            string previous = "#"; // initially there is no previous move
            if (index > 0) previous = filename + "." + (index - 1) + ".html"; // next move filename
            filename = filename + "." + index + ".html"; // combines the full filename
            Move[] lastMoves = new Move[5];
            lastMoves[0].row = lastMoves[1].row = lastMoves[2].row = lastMoves[3].row = lastMoves[4].row = -1; // guarantees initial out of board
            if (index > 0) // if we have some moves, the stack is not empty
            {
                int i;
                for (i = 0; i < 5; i++) // for the current move in the stack we have at least 1 move and not more than 5 (1 move + 4 neighbours)
                {
                    lastMoves[0] = OldMoves.Pop(); // extracts the lsst move from stack
                    if (lastMoves[0].type < 0) lastMoves[i+1] = lastMoves[0]; // this move is a result of neighbours destroy, move in the tail
                    else break; // since the exact move is pushed first into the stack
                }
                for (; i >= 0; i--) //returns back moves into the stack 
                    OldMoves.Push(lastMoves[i]);
            } //now we have in lastMoves[0] our last move and in some of lastMoves[1]..lastMoves[4] our last neighbour moves 

            try
            {
                outFile = new StreamWriter(filename); // opens the file for writing
            }
            catch (Exception e)
            {
                Console.WriteLine("Error! ", e.ToString());
                Console.WriteLine("Please restart the application");
                return false;
            }
            outFile.Write(@"
<html>
    <head>
        <title>Trolls Game Visualizer</title>
        <link rel=""stylesheet"" type=""text/css"" href=""styles.css"">
    </head>
    <body>
        <div class=""button-wrapper"">");
            if(previous.Length > 1) outFile.Write(@"
            <a href=""{0}"">Previous move</a>", previous);
            outFile.Write(@"
        </div>
        <div id=""next_div"" class=""button-wrapper"">");        
            if(next.Length > 1) outFile.Write(@"
            <a id=""next"" href=""{0}"">Next move</a>", next);
            outFile.Write(@"
        </div>
        <h1>Trolls Game Visualizer</h1>
        <h2>Move {0}</h2>
        <h3>",index);
            if (index>0)
            {
                if (lastMoves[0].type == 0) outFile.Write("TAKE");
                else outFile.Write("PUT");
                outFile.Write(" {0} {1}", lastMoves[0].row, lastMoves[0].column);
            } else outFile.Write("Initial content");
            outFile.Write(@"</h3>
        <div id=""container"">
            <table id=""Board"">
                <tr>
                    <td class=""head"">&nbsp;</td>");
            for (int i = 0; i < Board.GetLength(0); i++)
            {
                outFile.Write(@"
                    <td class=""head"">{0}</td>", i);         
            }
            outFile.Write(@" 
                </tr>");
            for (int r = 0; r < Board.GetLength(0); r++)
            {
            outFile.Write(@"
                <tr>
                    <td class=""head"">{0}</td>", r);
			    for (int c = 0; c < Board.GetLength(1); c++)
			    {
                    string moveType="";
                    if (lastMoves[0].row == r && lastMoves[0].column == c) moveType = " class=\"move\"";
                    else 
                        for (int i = 1; i < 5; i++)
                            if (lastMoves[i].row == r && lastMoves[i].column == c) moveType = " class=\"neighbour\"";
                     outFile.Write(@"
                    <td{1}>{0}</td>", Get(r, c), moveType);			 
			    }
                outFile.Write(@"
                </tr>");
            }
            outFile.Write(@"
            </table>
            <p><span class=""move"">&nbsp;&nbsp;&nbsp;&nbsp;</span> - the move's cell, 
               <span class=""neighbour"">&nbsp;&nbsp;&nbsp;&nbsp;</span> - the destroyed neighbours cells</p>
            <table>
                <tr>
                    <td>Initial board score:</td>
                    <td>{0}</td>
                    <td>&nbsp;&nbsp;</td>
                    <td>Current board score:</td>
                    <td>{1}</td>
                </tr>
                <tr>
                    <td>Points, earned to the moment:</td>
                    <td>{2}</td>
                    <td>&nbsp;&nbsp;</td>
                    <td>Points, earned at current move:</td>
                    <td>{3}</td>
                </tr>
            </table>
        </div>
        <hr />
        <h4>author: Pavel Sotirov</h4>
    </body>
</html>
            ", initScore, Score, initScore - Score, rank);
            outFile.Close(); // closes that file
            outFile.Dispose(); // releases all used resources 
            return true;
        }
    }

    class TrollsGameVisualizer
    {


        static void Main()
        {
            Console.WriteLine(@"
                 ****************************
                 *                          *
                 *  Trolls Game Visualizer  *
                 *                          *
                 *  author:  Pavel Sotirov  *
                 ****************************

Here you must enter input data as per the rules and conditions
set in the   ""Troll Game""   competition,  organized by Telerik
and PCMagazine. More details you can obtain here:
http://konkurs.pcmagbg.net/task-1-season-2012-2013/

The game visialization will proceed in the following steps:

1. Entering the number of moves, board dimensions and data.
You can select two ways to enter the board  - by using already
prepared text file  or  manually via the console.  If you omit
a filename when asked, that means you are choosing manual way.

2. Solving the board.
Please be patient!  Depending on board's dimensions and number
of desired moves, this process can take some time - up to four
minutes. You will be informed about the progress.

3. Creating output HTML files
For each move a corresponding  HTML  file will be created that
will hold the  board's content  after the move.  For each move
a separate file will be created with move's index.  Start file
with initial board content (0 index) also will be created. You
will be asked  to enter the proper filename.  If this filename
is omited or invalid, a default filename of  ""Trolls xxxx.html""
will be used (xxxx is move's index from 0 to 1000).

4. Finally the default browser will be invoked to show result.
            ");

            Console.Write("\n\nEnter input filename [empty for console]: ");
            string filename = Console.ReadLine(); // reads the filename from console

            StreamReader file = null; // since there is option to read data from file, opens empty StreamReader  
            bool hasFileData = false; // by default the data should be read from console
            if (filename.Length > 0) // but if the content is filename (and path)
            {
                hasFileData = true; // then all input data should be read from the file
                try
                {
                    file = new StreamReader(filename); // opens the file for reading
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error! ", e.ToString());
                    Console.WriteLine("Manual entering from console");
                    hasFileData = false;
                }
            }

            int C;
            int N;
            if (hasFileData) // reads from file
            {
                if (!int.TryParse(file.ReadLine(), out C) || C < 1 || C > 1000)
                {
                    Console.WriteLine("Invalid input file: wrong number of moves");
                    return;
                };
                if (!int.TryParse(file.ReadLine(), out N) || N < 1 || N > 1000)
                {
                    Console.WriteLine("Invalid input file: wrong board dimension");
                    return;
                };
            }
            else //reads from Console
            {
                do
                {
                    Console.Write("\nEnter number of moves to solve, C [1,1000]: ");
                } while (!int.TryParse(Console.ReadLine(), out C) || C < 1 || C > 1000);
                do
                {
                    Console.Write("\nEnter board dimensions N x N, N [2,1000]: ");
                } while (!int.TryParse(Console.ReadLine(), out N) || N < 2 || N > 1000);
                Console.WriteLine("\n\nPlease enter board data: {0} lines, {0} values per line, separated by space", N);
            }

            Playfield theBoard = new Playfield(N); // creates empty playfield
            for (int i = 0; i < N; i++) // iterates through rows
            {
                string[] Row;

                while (true)
                {
                    if (hasFileData) Row = file.ReadLine().Split(' '); // reads each line from file and separates it to array of string values 
                    else
                    {
                        Console.Write("Row {0, 4}: ", i);
                        Row = Console.ReadLine().Split(' '); // reads each line from console and separates it to array of string values 
                    }
                    if (Row.Length != N)
                    {
                        Console.WriteLine("\nWrong number of elements in Row {0}\n", i);
                        if (hasFileData) return; // if the input is from file stops the program
                        continue; // else goes to start of the loop
                    }
                    int j;
                    for (j = 0; j < N; j++) // iterates through cells in current row
                    {
                        int h = 0;
                        if (!int.TryParse(Row[j], out h) || h < 0 || h > Tower.maxHeight)
                        {
                            Console.WriteLine("\nWrong element value in Row {0}, Column {1}\n", i, j);
                            if (hasFileData) return; // if the input is from file stops the program
                            break; // else goes out of the loop
                        }
                        if (h > 0 && ((j > 0 && h == theBoard.Get(i, j - 1)) || (i > 0 && h == theBoard.Get(i - 1, j)))) //checks for duplicate heights in left and upper elements
                        {
                            Console.WriteLine("\nDuplicate height - Element in Row {0}, Column {1}\n", i, j);
                            if (hasFileData) return; // if the input is from file stops the program
                            break; // else goes out of the loop
                        }
                        theBoard.Set(i, j, h); // and pumps up the Playfield
                    }
                    if (j == N) break; // correct row data - exits the while loop
                }
            }

            if (hasFileData) // if the data source is file 
            {
                file.Close(); // closes that file
                file.Dispose(); // releases all used resources 
            }


            Console.Write("\n\nEnter output filename [up to 10 characters, only : ");
            while (true)
            {
                filename = Console.ReadLine(); // reads the filename from console               
                if (filename.Length < 1)
                {
                    Console.WriteLine("\nName too short\n");
                    continue;
                }
                if (filename.Length > 10)
                {
                    Console.WriteLine("\nName too long\n");
                    continue;
                }
                int i;
                for (i = 0; i < filename.Length; i++)
                    if (!char.IsLetter(filename[i]))
                    {
                        Console.WriteLine("\nInvalid filename\n");
                        break;
                    }
                if (i == filename.Length) break; // if filename is correct, goes out of the loop 
            }

            int initScore = theBoard.Score;
            int maxRank = 0; // the resultng rank of the best move, initially 0


            Console.WriteLine("Initial score: " + initScore);
            Move bestMove = new Move();
            //FULL WALKTROUGH - on each level (move's depth) the best result is selected
            for (int depth = 0; depth < C; depth++) // iterates through possible moves
            {
                if (!theBoard.PrintBoard(filename, depth, initScore, maxRank)) return; // prints the board into HTML file, returns on error

                maxRank = int.MinValue; // the resultng rank of the best move, initially the minimal possible value
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
            if (!theBoard.PrintBoard(filename, -C, initScore, maxRank)) return; // prints the last board into HTML file, returns on error (index is negative in order to inform the method this is the last file)
            Console.WriteLine("Final score: " + theBoard.Score);
            Console.Write("All files generated successfully!\nPress any key to continue to browser...\n");
            Console.ReadKey(true);
            Process.Start(filename + ".0.html");
        }



    }
}
