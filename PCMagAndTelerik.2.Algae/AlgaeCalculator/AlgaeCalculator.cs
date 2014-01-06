using System;
using System.IO;
using System.Linq;

namespace PCMagAndTelerik_2_Algae
{
    class AlgaeCalculator
    {
        static int inputT = 0; // number of turns [1,1000]
        static int inputN = 0; // puddle (square grid) dimension - length/width [3,1000]
        static int inputV = 0; // number of algaes that must be put into the puddle [3, N*N - 5]
        static int inputF = 0; // initial number of food pieces into the puddle [0, N*N]
        static char[][] field; // puddle data container
        static int topLeftFoodRow = 0; // the uppermost and leftmost coordinate of food region
        static int topLeftFoodCol = 0;
        static int bottomRightFoodRow = 1000; // the bottommost and rightmost coordinate of food region
        static int bottomRightFoodCol = 1000;

        static void Main()
        {
            string file = Console.ReadLine();
            if (!int.TryParse(file, out inputT))
            // if the first input value is not correct, then read data from external file (DEBUG MODE)
            {
                if (file == "@") // entering "@" as a first value means using of default file "test.txt"
                {
                    file = "..\\..\\..\\AlgaeGenerator\\bin\\Release\\test.txt";
                }
                else // otherwise only appends path to the tests generator folder
                {
                    file = "..\\..\\..\\AlgaeGenerator\\bin\\Release\\" + file;
                }

                ReadPuddle(file); // reads input data from file
            }
            else
            {
                ReadPuddle(); // reads input data from Console             
            }

            // main algo
            double density = inputV / (double)(inputN * inputN);
            if (density < 0.44) // the max density of sequence of block elements is 0.44 (4 algaes in 9 cells grid) 
            {
                if (density > 0.32 && inputN > 17)
                // fill uniformly with blocks and half-beacons
                {
                    FillBlocks();
                }
                else if (density > 0.047 && density < 0.12 && inputN > 18)
                // fill uniformly with crosses (exploders) but only over big field
                {
                    FillCrosses();
                }
                else if (inputF < inputV && inputF > 0)
                // the food pieces are less than algaes but only if there is a food
                {
                    FillOverFood();
                }
                else
                // the food pieces are higher that algaes
                // puts suitable exploder in the center and as many blinkers as possible arround 
                {
                    FillExploderAndBlinkers();
                }
            }
            else
            {
                // if density is greater than 44% fill the maximum region with 0.5 density (x - pattern) and the rest with 1.0 denisty
                FillDensePuddle();
            }

            // end of main algo

            //outputs the result
            for (int i = 0; i < inputN; i++)
            {
                Console.WriteLine(field[i]); 
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        static void ReadPuddle (string filename = null)
        {
            StreamReader inFile = null;
            bool fromFile = false;
            if (filename !=  null)
            {
                fromFile = true;
                inFile = new StreamReader(filename);
                inputT = int.Parse(inFile.ReadLine());
            }

            inputV = int.Parse((fromFile) ? inFile.ReadLine() : Console.ReadLine()); // reads algaes nr.
            inputN = int.Parse((fromFile) ? inFile.ReadLine() : Console.ReadLine()); // reads puddle dimension

            topLeftFoodCol = inputN-1; // sets the food region using their least minimum coordinates values
            topLeftFoodRow = inputN-1;
            bottomRightFoodRow = 0;
            bottomRightFoodCol = 0;

            field = new char[inputN][]; // creates N rows of puddle data

            for (int row = 0; row < inputN; row++) // iterates trough puddle rows
            {
                field[row] = new char[inputN]; // creates N columns of puddle data
                string line = (fromFile) ? inFile.ReadLine() : Console.ReadLine(); //reads row of puddle data
                line.CopyTo(0, field[row], 0, inputN); // fills the whole row of puddle data into the array
                int idx = line.IndexOf('F'); // looks if there is a food on current row of data
                if (idx >= 0)
                {
                    if (topLeftFoodCol > idx) topLeftFoodCol = idx;
                    if (topLeftFoodRow > row) topLeftFoodRow = row;
                    if (bottomRightFoodCol < idx) bottomRightFoodCol = idx;
                    if (bottomRightFoodRow < row) bottomRightFoodRow = row;
                }
                while (idx >= 0) // iterates through each food cell into the current row
                {
                    inputF++; // counts food cells
                    idx++;
                    if (idx < line.Length) idx = line.IndexOf('F', idx); // goes to the next food cell
                    else idx = -1;
                    if (idx >= 0)
                    {
                        if (bottomRightFoodCol < idx) bottomRightFoodCol = idx;
                        if (bottomRightFoodRow < row) bottomRightFoodRow = row;
                    }
                }
            }

            if (fromFile) // if readings are from file, closes it
            {
                inFile.Dispose();
                inFile.Close();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        static void FillDensePuddle()
        {
            // fill the maximum region with 0.5 density (x - pattern) and the rest with 1.0 denisty

            // calculates the required region with denisty 0.5 (N*N-V is number of empty cells)
            // this region must have odd rows/columns in order to have chance to obtain stable figure
            int halfDensityN = (int)Math.Sqrt(2 * (inputN * inputN - inputV));
            if (halfDensityN >= inputN) halfDensityN = inputN-1; // ensures not to exceed the range 
            halfDensityN = (halfDensityN - 1) / 2 * 2 + 1; // makes to be odd


            // full uniform bottom right region first
            for (int i = halfDensityN; i < inputN - 1; i++)
            {
                for (int j = halfDensityN; j < inputN; j++)
                {
                    field[j][i] = '+';
                    inputV--;
                }
            }
            // in order to ensure symmetry we have to miss the very last algae if V is even and this region has odd elements
            int corr = ((inputV % 2 == 0) && ((inputN - halfDensityN) % 2 == 1)) ? 1 : 0;
            for (int j = halfDensityN; j < inputN - corr; j++) // last line of bottom end region
            {
                field[j][inputN - 1] = '+';
                inputV--;
            }

            // fills upper-top region with x-pattern - 0.5 density
            for (int idx = 0; idx < halfDensityN; idx++) // moves on matrix diagonal
                for (int ofs = 0; (ofs < halfDensityN && inputV > 0); ofs++) // inner loop only if there is algaes to put
                {
                    if ((ofs - idx) % 2 == 1)
                    {
                        field[idx][ofs] = '+';
                        field[ofs][idx] = '+';
                        inputV -= 2;
                    }
                }

            // and finally fills uniformly the rest regions (upper-right and bottom-left) with the rest of algaes
            for (int i = inputN - 1; i >= halfDensityN; i--)
                for (int j = 0; (j < halfDensityN && inputV > 0); j++)
                {
                    field[j][i] = '+';
                    inputV--;
                    if (inputV > 0)
                    {
                        field[i][j] = '+';
                        inputV--;
                    }
                }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        static void FillBlocks()
        {
            for (int row = 0; row < inputN-1; row += 3) // fills uniformly with half-beacons - 3 algaes
                for (int col = 0; (col < inputN-1 && inputV > 2); col += 3)
                {
                    field[row][col] = '+';
                    field[row+1][col] = '+';
                    field[row][col+1] = '+';
                    inputV -= 3;
                }

            if (inputV == 0) return; // finished only with half-beacons 
            for (int row = 1; row < inputN; row += 3) // adds algae in each half-beacon to form block until free algaes finish
                for (int col = 1; col < inputN; col += 3)
                {
                    field[row][col] = '+';
                    if (--inputV == 0) return; // returns if ready
                }

            for (int col = 0; col < inputN; col++) // in rare cases if have more free algaes puts all of them at the bottom and at the right 
            {
                field[inputN-1][col] = '+';
                if (--inputV == 0) return;
                field[col][inputN-1] = '+';
                if (--inputV == 0) return;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        static void FillCrosses()
        {
            for (int row = 4; row < inputN-2; row += 10) // fills uniformly with crosses - 5 algaes
                for (int col = 4; (col < inputN-2 && inputV > 2); col += 10)
                {
                    field[  row][  col] = '+'; // cross
                    field[row+1][  col] = '+';
                    field[row-1][  col] = '+';
                    field[  row][col+1] = '+';
                    field[  row][col-1] = '+';
                    inputV -= 5;
                }

            if (inputV == 0) return; // finished only with crosses 
            for (int row = 9; row < inputN-1; row += 10) // adds 5-algaes X-patterns in the center of each 4 crosses until algaes finish
                for (int col = 9; (col < inputN-1 && inputV > 4); col += 10)
                {
                    field[  row][  col] = '+'; // X-pattern
                    field[row-1][col-1] = '+';
                    field[row-1][col+1] = '+';
                    field[row+1][col-1] = '+';
                    field[row+1][col+1] = '+';
                    inputV -= 5;
                }

            if (inputV == 0) return; // finished only with additonal X-patterns
            for (int col = 0; col < inputN-1; col += 2) // in rare cases if have more free algaes puts all of them at the sides 
            {
                field[0][col+1] = '+';
                if (--inputV == 0) return;
                field[col+1][0] = '+';
                if (--inputV == 0) return;
                field[inputN - 1][col+1] = '+';
                if (--inputV == 0) return;
                field[col+1][inputN - 1] = '+';
                if (--inputV == 0) return;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        static void FillOverFood()
        {
            // initially walks through the food region and looks for food cells only
            FillHalfBeaconsInRegion(topLeftFoodRow, topLeftFoodCol, bottomRightFoodCol, bottomRightFoodRow);

            // after if we have  enough algaes, walks through the whole puddle and looks for empty cells
            if (inputV > 3) FillHalfBeaconsInRegion(0, 0, inputN-1, inputN-1, false);
            
            int col = 0; // leaves one row space between last food line and current position
            int row = bottomRightFoodRow + 2;
            if (row > inputN - 1) row = inputN-1; // in any case avoids out of range error
            while (inputV > 0) // put additional algaes without any pattern if needed 
            {
                if (field[row][col] != '+')
                {
                    field[row][col] = '+'; // just puts algae
                    inputV--;
                }
                col++; // goes to the next possible position
                if (col > inputN - 1)
                {
                    col = 0;
                    row++;
                    if (row > inputN - 1) row = 0;
                }
            }
        }

        // Internal walk-trough methood
        static void FillHalfBeaconsInRegion(int top, int left, int right, int bottom, bool onlyFood = true)
        {
            for (int row = top; row <= bottom; row++)
                for (int col = left; col <= right; col++)
                {
                    if ((row > 0 && (field[row - 1][col] == '+' || ( col < inputN-1 && field[row - 1][col+1] == '+')) ) || // if there is neighbour in the top cell or top right cell
                            (col > 0 && field[row][col - 1] == '+') ||  // if there is neighbour in the left cell
                            (col < inputN - 2 && field[row][col + 1] == '+') || // if there is neighbour in the right cell
                            (col < inputN - 3 && field[row][col + 2] == '+')) // if there is neighbour in the cell after the right cell
                                continue; // go ahead 
                    if (onlyFood && field[row][col] != 'F') continue; // if we are looking for food cels then go ahead 
                    if (row < inputN - 1 && col < inputN - 1) // BINGO! we have found the right cell and we can place the half-beacon around int
                    {
                        field[row + 1][col] = '+'; // half-beacon
                        field[row][col + 1] = '+';
                        field[row + 1][col + 1] = '+';
                        inputV -= 3; // counts down
                        if (inputV < 3) return; // we don't have enough algaes to continue
                        col += 2; // goes to the next available position (there must be space between blocks)
                    }
                }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        static void FillExploderAndBlinkers()
        {
            //this method puts the exploder in the center and fills around with blinkers
            int exploderWidth = 14; // initial dimension of the central square region, left for the exploder

            // first we must be sure that after putting of exploder an integer number of blinkers should left (i.e. algaes multiple to 3 cells)
            if (inputV % 9 == 2) // one or two algaes left are lost in any case
            {
                field[inputN-1][inputN-1] = '+'; // leave one algae at the bottom-right corner
                inputV--;
            }
            if (inputV % 9 == 1) // one algaes left is lost in any case
            {
                field[0][0] = '+'; // leave one algae at the top-left corner
                inputV--;
            }
            switch (inputV % 9) // the rest possiblities for central exploder
            {
                case 0: // blinker, p2, w5x5
                case 3: // 1 central blinker + 2 blinkers after (9-3= 2x3)
                    exploderWidth = 5;
                    field[inputN/2  ][inputN/2-1] = '+'; // ***
                    field[inputN/2  ][inputN/2  ] = '+';
                    field[inputN/2  ][inputN/2+1] = '+'; 
                    inputV -= 3;            
                    break;
                case 4: // T-exploder, p10, w11x11
                    exploderWidth = 11;
                    field[inputN/2-1][inputN/2  ] = '+'; 
                    field[inputN/2  ][inputN/2-1] = '+'; //  *
                    field[inputN/2  ][inputN/2  ] = '+'; // ***
                    field[inputN/2  ][inputN/2+1] = '+'; 
                    inputV -= 4;
                    break;
                case 5: // R-pentomino, p1103, w(doesn't matter) = w14x14
                case 8: // R-pentomino + 1 blinker after (8-5=3)
                    field[inputN/2-1][inputN/2+1] = '+'; //  **
                    field[inputN/2-1][inputN/2  ] = '+'; // **
                    field[inputN/2  ][inputN/2  ] = '+'; //  *
                    field[inputN/2  ][inputN/2-1] = '+';
                    field[inputN/2+1][inputN/2  ] = '+'; 
                    inputV -= 5;
                    break;
                case 6: // "citroen" exploder ("very nice indeed", fuck the French cars), p8, w11x11
                    exploderWidth = 11;
                    field[inputN/2-2][inputN/2  ] = '+'; //  *
                    field[inputN/2-1][inputN/2-1] = '+'; // * *
                    field[inputN/2-1][inputN/2+1] = '+'; //  
                    field[inputN/2+1][inputN/2  ] = '+'; //  *
                    field[inputN/2+2][inputN/2-1] = '+'; // * *
                    field[inputN/2+2][inputN/2+1] = '+';
                    inputV -= 6;
                    break;
                case 7: // small exploder, p17, w15x15
                    exploderWidth = 15;
                    field[inputN/2-1][inputN/2  ] = '+'; //  *
                    field[inputN/2  ][inputN/2-1] = '+'; // ***
                    field[inputN/2  ][inputN/2  ] = '+'; // * * 
                    field[inputN/2  ][inputN/2+1] = '+'; //  *
                    field[inputN/2+1][inputN/2-1] = '+'; 
                    field[inputN/2+1][inputN/2+1] = '+'; 
                    field[inputN/2+2][inputN/2  ] = '+';
                    inputV -= 7;
                    break;
                default: break;
            }
            if (((inputN - exploderWidth) / 2 - topLeftFoodRow) < 3) //overrides top-left food coordinates if falls wihtin exploder window
            {
                topLeftFoodRow = 1; // first row must be empty for the blinkers to transform
                topLeftFoodCol = 0;
            } else if (((inputN - exploderWidth)/2 - topLeftFoodCol)  < 3) topLeftFoodCol=0; // otherwise overrides left coodinate if there is no enogh space
 
            for (int row = topLeftFoodRow; row < inputN-1; row += 4) // last row must be empty for the blinkers to transform
                for (int col = topLeftFoodCol; col < inputN-2; col += 4)
                // step is 4 in order to achieve enough spaces, N-2 ensures that blinker will not exceed the right limit
                {
                    if (field[row][col] == '+' || field[row][col + 1] == '+' || // if there is neighbour in the blinker cells
                            field[row][col + 2] == '+') continue; // go ahead
                    if (row > (inputN - exploderWidth)/2 && row < (inputN + exploderWidth)/2 && // row is within exploder space
                        col > (inputN - exploderWidth)/2 && col < (inputN + exploderWidth)/2) continue; // and col is within also - go ahead
                        field[row][col    ] = '+'; // horizontal blinker
                        field[row][col + 1] = '+';
                        field[row][col + 2] = '+';
                        inputV -= 3; // counts down algaes number
                        if (inputV == 0) return; // when all algaes are in place, just finish
                 }

            // if we are here, obviously there is no space for blinkers, put the rest algaes anywhere on free cells
            int r = inputN - 1; // last probably empty cell to iterate 
            int c = inputN - 2;
            while (inputV > 0)
            {
                if (field[r][c] != '+')
                {
                    field[r][c] = '+';
                    inputV--;
                }
                c--;
                if (c < 0)
                {
                    c = 0;
                    r--;
                }
            }
        }
    }
}
