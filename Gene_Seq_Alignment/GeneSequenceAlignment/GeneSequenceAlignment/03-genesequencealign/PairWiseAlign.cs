using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GeneticsLab
{
    class PairWiseAlign
    {
        int MaxCharactersToAlign;

        public PairWiseAlign()
        {
            // Default is to align only 5000 characters in each sequence.
            this.MaxCharactersToAlign = 5000;
        }

        public PairWiseAlign(int len)
        {
            // Alternatively, we can use an different length; typically used with the banded option checked.
            this.MaxCharactersToAlign = len;
        }

        /// <summary>
        /// this is the function you implement.
        /// </summary>
        /// <param name="sequenceA">the first sequence</param>
        /// <param name="sequenceB">the second sequence, may have length not equal to the length of the first seq.</param>
        /// <param name="banded">true if alignment should be band limited.</param>
        /// <returns>the alignment score and the alignment (in a Result object) for sequenceA and sequenceB.  The calling function places the result in the dispay appropriately.
        /// 
        public ResultTable.Result Align_And_Extract(GeneSequence sequenceA, GeneSequence sequenceB, bool banded)
        {
            ResultTable.Result result = new ResultTable.Result();
            int score;                                                       // place your computed alignment score here
            string[] alignment = new string[2];                              // place your two computed alignments here

            MyAligner aligner = new MyAligner(sequenceA.Sequence, sequenceB.Sequence, banded, MaxCharactersToAlign);
;
            aligner.ExecuteAlignment();


            // ********* these are placeholder assignments that you'll replace with your code  *******
            score = aligner.GetCost();
            alignment[0] = aligner.GetAlignedSequenceA();
            alignment[1] = aligner.GetAlignedSequenceB();
            // ***************************************************************************************
            

            result.Update(score,alignment[0],alignment[1]);                  // bundling your results into the right object type 
            return(result);
        }
    }

    class MyAligner
    {
        private string sequence1;
        private string sequence2;
        private int iMax;
        private int jMax;
        private bool banded = false;
        private int cost;
        private string lowCostSeq1;
        private string lowCostSeq2;


        static int d = 3;
        static int nil = -1;
        static int bandwidth = 2 * d + 1;
        static string dash = "-";
        static int indelCost = 5;
        static int subCost = 1;
        static int matchCost = -3;
        static int printLength = 100;


        //Constructor
        public MyAligner(string sequenceA, string sequenceB, bool banded, int characterMax)
        {
            this.sequence1 = dash + sequenceA;
            this.sequence2 = dash + sequenceB;

            int size1 = this.sequence1.Length > (characterMax + 1) ? (characterMax +1) : this.sequence1.Length;
            int size2 = this.sequence2.Length > (characterMax + 1) ? (characterMax + 1) : this.sequence2.Length;

            this.iMax = size1;
            this.jMax = size2;

            this.banded = banded;

            this.cost = 0;
            this.lowCostSeq1 = "";
            this.lowCostSeq2 = "";

        }

        public void ExecuteAlignment()
        {
            if (!this.banded)
            {
                this.cost = AlignUnrestricted(this.sequence1, this.sequence2);
            }
            else
            {
                this.cost = AlignBanded(this.sequence1, this.sequence2);
            }
        }

        public int AlignUnrestricted(string seq1, string seq2)
        {
            //Create and Initialize Matrices
            int[,] costMatrix = new int[this.iMax, this.jMax];
            Point[,] prevMatrix = new Point[this.iMax, this.jMax];
            InitializeUnrestrictedMatrices(ref costMatrix, ref prevMatrix);

            //Find the lowest cost and get the strings
            string finalSeq1 = "";
            string finalSeq2 = "";
            int finalCost = 0;
            FindLowestCost(ref costMatrix, ref prevMatrix, ref finalSeq1, ref finalSeq2, ref finalCost);

            this.lowCostSeq1 = finalSeq1;
            this.lowCostSeq2 = finalSeq2;
            return finalCost;
        }

        public int AlignBanded(string seq1, string seq2)
        {
            if(AlignmentPossibleWithBand(this.iMax, this.jMax))
            {
                //Create and Initialize Banded Matrices
                int[,] costMatrix = new int[this.iMax, this.jMax];
                Point[,] prevMatrix = new Point[this.iMax, this.jMax];
                InitializeBandedMatrices(ref costMatrix, ref prevMatrix);

                //Find the lowest cost and get the strings
                string finalSeq1 = "";
                string finalSeq2 = "";
                int finalCost = 0;
                FindLowestBandedCost(ref costMatrix, ref prevMatrix, ref finalSeq1, ref finalSeq2, ref finalCost);

                this.lowCostSeq1 = finalSeq1;
                this.lowCostSeq2 = finalSeq2;
                return finalCost;
            }
            else
            {
                this.lowCostSeq1 = "No Alignment Possible";
                this.lowCostSeq2 = "No Alignment Possible";
                return int.MaxValue;
            }
        }

        public bool AlignmentPossibleWithBand(int length1, int length2)
        {
            if(Math.Abs(length1-length2) > d || Math.Abs(length2 - length1) > d)
            {
                return false; // Not possible
            }
            else
            {
                return true; // Possible
            }
        }

        public void InitializeUnrestrictedMatrices(ref int[,] costMatrix, ref Point[,] prevMatrix)
        {
            //Assuming each string is at least 3 characters long
            for (int i = 0; i < this.iMax; i++)
            {
                if (i == 0)
                {
                    costMatrix[0, 0] = 0;
                    prevMatrix[0, 0] = new Point(nil, nil);
                    continue;
                }
                costMatrix[i, 0] = indelCost + costMatrix[i - 1, 0];
                prevMatrix[i, 0] = new Point(i - 1, 0);
            }

            for (int j = 1; j < this.jMax; j++)
            {
                costMatrix[0, j] = indelCost + costMatrix[0, j - 1];
                prevMatrix[0, j] = new Point(0, j - 1);
            }
        }

        public void InitializeBandedMatrices(ref int[,] costMatrix, ref Point[,] prevMatrix)
        {
            for (int i = 0; i < d + 1; i++)
            {
                if (i == 0)
                {
                    costMatrix[0, 0] = 0;
                    prevMatrix[0, 0] = new Point(nil, nil);
                    continue;
                }
                costMatrix[i, 0] = indelCost + costMatrix[i - 1, 0];
                prevMatrix[i, 0] = new Point(i - 1, 0);
            }

            for (int j = 1; j < d + 1; j++)
            {
                costMatrix[0, j] = indelCost + costMatrix[0, j - 1];
                prevMatrix[0, j] = new Point(0, j - 1);
            }
        }

        public void FindLowestCost(ref int[,] costMatrix, ref Point[,] prevMatrix, ref string string1, ref string string2, ref int cost)
        {
            bool top = false;
            bool left = false;
            bool diagonal = false;

            for (int i = 1; i < this.iMax; i++)
            {
                for (int j = 1; j < this.jMax; j++)
                {

                    costMatrix[i, j] = GetDynamicCost(i, j, ref top, ref left, ref diagonal, ref costMatrix);
                    prevMatrix[i, j] = GetPreviousCoords(i, j, ref top, ref left, ref diagonal);
                    top = false;
                    left = false;
                    diagonal = false;

                }
            }

            TraceBack(ref string1, ref string2, ref prevMatrix);
            string1 = ReverseString(string1);
            string2 = ReverseString(string2);
            cost = costMatrix[iMax-1, jMax-1];
        }

        public void FindLowestBandedCost(ref int[,] costMatrix, ref Point[,] prevMatrix, ref string string1, ref string string2, ref int cost)
        {

            for(int i = 1; i < this.iMax; i++)
            {
                ExecuteBandedRow(i, MakeColumns(i), ref costMatrix, ref prevMatrix);
            }

            TraceBack(ref string1, ref string2, ref prevMatrix);
            string1 = ReverseString(string1);
            string2 = ReverseString(string2);
            cost = costMatrix[iMax - 1, jMax - 1];
        }

        public void ExecuteBandedRow(int i, List<int> columns, ref int[,] costMatrix, ref Point[,] prevMatrix)
        {
            bool top = false;
            bool left = false;
            bool diagonal = false;


            //For each column j in the list
            int numberOfColumns = columns.Count;
            bool jFirst = true;
            bool jLast = false;
            bool iIsGreaterThanD = i > d ? true : false;
            bool columnsEqualBandwidth = numberOfColumns == bandwidth ? true : false;
            foreach (int j in columns)
            {
                if(j == columns[numberOfColumns-1])
                {
                    jLast = true;
                }
                costMatrix[i, j] = GetBandedDynamicCost(i, j, ref top, ref left, ref diagonal, ref costMatrix, ref jFirst, iIsGreaterThanD, jLast, columnsEqualBandwidth);
                prevMatrix[i, j] = GetPreviousCoords(i, j, ref top, ref left, ref diagonal);
                top = false;
                left = false;
                diagonal = false;

            }
        }

        public List<int> MakeColumns(int i)
        {
            int j = i - d;
            List<int> columns = new List<int>();
            for(int iteration = 0; iteration < bandwidth; iteration++)
            {
                if(j < 1)
                {
                    j++;
                    continue;
                }
                if(j > this.jMax - 1)
                {
                    continue;
                }

                columns.Add(j);
                j++;            
            }
            return columns;
        }

        public int GetDynamicCost(int i, int j, ref bool top, ref bool left, ref bool diagonal, ref int[,] costMatrix)
        {
            //Current spot is costMatrix[i, j]

            int topCost = costMatrix[i, j - 1] + indelCost;
            int leftCost = costMatrix[i - 1, j] + indelCost;
            int diagCost = CalculateDiagonalCost(i, j, costMatrix[i - 1, j - 1]);

            return MinimumOf(topCost, leftCost, diagCost, ref top, ref left, ref diagonal);
        }

        public int GetBandedDynamicCost(int i, int j, ref bool top, ref bool left, ref bool diagonal, ref int[,] costMatrix, ref bool jFirst, bool iGreaterThanD, bool jLast, bool columnsEqualBandwidth)
        {
            //Current spot is costMatrix[i, j]

            int topCost = 0;
            int leftCost = 0;
            int diagCost = 0;

            if (jFirst && iGreaterThanD)
            {
                //Then we need to pull from the left or diagonal NOT TOP (TOP IS OUT OF RANGE)
                leftCost = costMatrix[i - 1, j] + indelCost;
                diagCost = CalculateDiagonalCost(i, j, costMatrix[i - 1, j - 1]);
                //Make top bigger than the other values
                topCost = int.MaxValue;
                jFirst = false;

            }
            else if ((jLast && columnsEqualBandwidth) || (jLast && !iGreaterThanD))
            {
                //Then we need to pull from the top or diagonal NOT LEFT (LEFT IS OUT OF RANGE)
                topCost = costMatrix[i, j - 1] + indelCost;
                diagCost = CalculateDiagonalCost(i, j, costMatrix[i - 1, j - 1]);
                //Make left bigger than the other values
                leftCost = int.MaxValue;
                jLast = false;

            }
            else
            {
                //If we get to here we know we are not out of range
                topCost = costMatrix[i, j - 1] + indelCost;
                leftCost = costMatrix[i - 1, j] + indelCost;
                diagCost = CalculateDiagonalCost(i, j, costMatrix[i - 1, j - 1]);
            }


            return MinimumOf(topCost, leftCost, diagCost, ref top, ref left, ref diagonal);
        }

        public int MinimumOf(int topCost, int leftCost, int diagCost, ref bool top, ref bool left, ref bool diagonal)
        {
            //If they are all different
            if (topCost < leftCost && topCost < diagCost)
            {
                top = true;
                return topCost;
            }
            if (leftCost < topCost && leftCost < diagCost)
            {
                left = true;
                return leftCost;
            }
            if (diagCost < topCost && diagCost < leftCost)
            {
                diagonal = true;
                return diagCost;
            }

            //If two are the same
            if(topCost == diagCost && topCost < leftCost)
            {
                diagonal = true;
                return diagCost;
            }
            if(leftCost == diagCost && leftCost < topCost)
            {
                diagonal = true;
                return diagCost;
            }
            if (topCost == leftCost && topCost < diagCost)
            {
                top = true;
                return topCost;
            }

            //If three are the same
            if (leftCost == topCost && topCost == diagCost)
            {
                diagonal = true;
                return diagCost;
            }

            //Shouldn't ever get here
            return 0;
        }

        public int CalculateDiagonalCost(int i, int j, int diagPrevious)
        {
            //If the characters are equal
            if (this.sequence1[i] == this.sequence2[j])
            {
                return matchCost + diagPrevious;
            }
            else
            {
                return subCost + diagPrevious;
            }


        }

        public Point GetPreviousCoords(int i, int j, ref bool top, ref bool left, ref bool diagonal)
        {
            if(top)
            {
                return new Point(i, j - 1);
            }
            else if(left)
            {
                return new Point(i - 1, j);
            }
            else//Diagonal
            {
                return new Point(i - 1, j - 1);
            }
            
        }

        public void TraceBack(ref string string1, ref string string2, ref Point[,] prevMatrix)
        {
            int i = 0;
            int j = 0;
            int iPrev = this.iMax - 1;
            int jPrev = this.jMax - 1;
            bool top = false;
            bool left = false;
            bool diagonal = false;
            int greatestSeqLength = iMax > jMax ? iMax : jMax;

            //Initially starts at lowest cost
            string1 += this.sequence1[iPrev];
            string2 += this.sequence2[jPrev];

            i = prevMatrix[iPrev, jPrev].X;
            j = prevMatrix[iPrev, jPrev].Y;


            do
            {
                DetermineCostType(i, j, iPrev, jPrev, ref top, ref left, ref diagonal);
                if (top)
                {
                    string1 += dash;
                    string2 += this.sequence2[j];
                }
                else if (left)
                {
                    string1 += this.sequence1[i];
                    string2 += dash;
                }
                else
                {
                    string1 += this.sequence1[i];
                    string2 += this.sequence2[j];
                }


                iPrev = i;
                jPrev = j;
                i = prevMatrix[iPrev, jPrev].X;
                j = prevMatrix[iPrev, jPrev].Y;
                top = false;
                left = false;
                diagonal = false;

            } while (i != -1 && j != -1);
        }

        public string ReverseString(string str)
        {
            int length = str.Length - 1;
            string reversedString = "";

            for(int i = length; i > -1; i--)
            {
                reversedString += str[i];
            }
            if(str.Length >= printLength)
            {
                length = printLength + 2;
            }
            else
            {
                length = str.Length;
            }
            return reversedString.Substring(1, length-1);
        }

        public void DetermineCostType(int i, int j, int iPrev, int jPrev, ref bool top, ref bool left, ref bool diagonal)
        {
            //TOP type
            if(i == iPrev)
            {
                top = true;
            }//LEFT
            else if(j == jPrev)
            {
                left = true;
            }
            else//DIAGONAL
            {
                diagonal = true;
            }
        }

        public int GetCost()
        {
            return this.cost;
        }
        public string GetAlignedSequenceA()
        {
            return this.lowCostSeq1;
        }
        public string GetAlignedSequenceB()
        {
            return this.lowCostSeq2;
        }
    }
}



