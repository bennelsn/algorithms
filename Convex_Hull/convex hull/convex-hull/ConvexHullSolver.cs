using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;

namespace _2_convex_hull
{
    class ConvexHullSolver
    {
        System.Drawing.Graphics g;
        System.Windows.Forms.PictureBox pictureBoxView;

        public ConvexHullSolver(System.Drawing.Graphics g, System.Windows.Forms.PictureBox pictureBoxView)
        {
            this.g = g;
            this.pictureBoxView = pictureBoxView;
        }

        public void Refresh()
        {
            // Use this especially for debugging and whenever you want to see what you have drawn so far
            pictureBoxView.Refresh();
        }

        public void Pause(int milliseconds)
        {
            // Use this especially for debugging and to animate your algorithm slowly
            pictureBoxView.Refresh();
            System.Threading.Thread.Sleep(milliseconds);
        }

        public void Solve(List<System.Drawing.PointF> pointList)
        {
            //Sort Points correctly based of the x value.
            pointList = Sort(pointList);

            //Find the convex hull of the list of points
            ConvexHull finalHull = FindConvexHull(pointList);

            //Draw the convex hull that was found.
            DrawConvexHull(finalHull);
        }

        public void DrawConvexHull(ConvexHull ch)
        {
            Pen pen = new Pen(Color.Red, 1.0f);

            //Draw all connecting
            for (int i = 0; i < ch.GetSize() - 1; i++)
            {
                g.DrawLine(pen, ch.At(i), ch.At(i+1));
            }
            //Draw the last line
            g.DrawLine(pen, ch.Last(), ch.First());

        }

        public List<PointF> Sort(List<PointF> list)
        {
            return list.OrderBy(p => p.X).ToList();
        }

        public List<PointF> GetPointsWithinRange(int begin, int end, List<PointF> list)
        {
            List<PointF> listToReturn = new List<PointF>();

            for(int i = begin; i < end; i++)
            {
                listToReturn.Add(list[i]);
            }

            return listToReturn;
        }

        public PointF GetPointFromHull(string decider, ConvexHull convexHull)
        {
            if(decider == "RIGHTMOST")
            {
                List<PointF> list = Sort(convexHull.GetConvexHullPoints());
                return list[list.Count - 1];
            }
            if(decider == "LEFTMOST")
            {
                List<PointF> list = Sort(convexHull.GetConvexHullPoints());
                return list[0];
            }

            return new PointF();
        }

        public double CalculateSlope(PointF p1, PointF p2)
        {
            //y2-y1/x2-x1
            return (p2.Y - p1.Y) / (p2.X - p1.X);

        }

        public int Previous(List<PointF> list, int index)
        {
            if((index - 1) < 0)
            {
                return list.Count - 1;
            }
            else
            {
                return index - 1;
            }
        }

        public int Previous(List<PointF> list, int index, ref bool sameIndexWasReturned)
        {
            if ((index - 1) < 0)
            {
                sameIndexWasReturned = ((list.Count - 1) == index) ? true : false;
                return list.Count - 1;
            }
            else
            {
                sameIndexWasReturned = false;
                return index - 1;
            }
        }

        public int Next(List<PointF> list, int index)
        {
            if ((index + 1) > (list.Count - 1))
            {
                return 0; //jump to beginning of the list
            }
            else
            {
                return index + 1;
            }
        }

        public int Next(List<PointF> list, int index, ref bool sameIndexWasReturned)
        {
            if ((index + 1) > (list.Count - 1))
            {
                sameIndexWasReturned = (index == 0) ? true : false;
                return 0; //jump to beginning of the list
            }
            else
            {
                sameIndexWasReturned = false;
                return index + 1;
            }
        }

        public void CounterClockwiseTangentMove(ref ConvexHull hull, ref TangentLine TLine, ref double slope, ref int slopeChangedCount, string TangentType)
        {
            
            bool slopeChanged = false;
            string LOWER_TANGENT = "LOWER_TANGENT";
            do
            {
                int index;
                PointF pendingTanPoint;
                double pendingSlope;
                if (TangentType == LOWER_TANGENT)
                {
                    //TangentRightPoint will move to the next point in a COUNTER-CLOCKWISE direction
                    float rightPointX = TLine.GetRightPoint().X;
                    index = hull.GetConvexHullPoints().FindIndex(p => p.X == rightPointX);

                    //Start at the index, so to go COUNTER-CLOCKWISE we need to move previous through the list
                    int i = Previous(hull.GetConvexHullPoints(), index);

                    pendingTanPoint = hull.GetConvexHullPoints()[i];
                    pendingSlope = CalculateSlope(TLine.GetLeftPoint(), pendingTanPoint);
                }
                else
                {
                    //TangentLeftPoint will move to the next point in a COUNTER-CLOCKWISE direction
                    float leftPointX = TLine.GetLeftPoint().X;
                    index = hull.GetConvexHullPoints().FindIndex(p => p.X == leftPointX);

                    //Start at the index, so to go COUNTER-CLOCKWISE we need to move previous through the list
                    int i = Previous(hull.GetConvexHullPoints(), index);

                    pendingTanPoint = hull.GetConvexHullPoints()[i];
                    pendingSlope = CalculateSlope(pendingTanPoint, TLine.GetRightPoint());
                }


                if (pendingSlope < slope)
                {
                    //That's good, so now we can change the tangent line
                    if (TangentType == LOWER_TANGENT)
                    {
                        TLine.SetRightPoint(pendingTanPoint);
                    }
                    else
                    {
                        TLine.SetLeftPoint(pendingTanPoint);
                    }
                    //And also update the slope
                    slope = pendingSlope;
                    //And the slope changed
                    slopeChanged = true;
                    slopeChangedCount++;
                }
                else
                {
                    slopeChanged = false;
                }

            } while (slopeChanged);
        }

        public void ClockwiseTangentMove(ref ConvexHull hull, ref TangentLine TLine, ref double slope, ref int slopeChangedCount, string TangentType)
        {
            bool slopeChanged = false;
            string LOWER_TANGENT = "LOWER_TANGENT";
            do
            {

                int index;
                PointF pendingTanPoint;
                double pendingSlope;
                if (TangentType == LOWER_TANGENT)
                {
                    //TangentLeftPoint will move to the next point in a CLOCKWISE direction
                    float leftPointX = TLine.GetLeftPoint().X;
                    index = hull.GetConvexHullPoints().FindIndex(p => p.X == leftPointX);

                    //Start at the index, so to go CLOCKWISE we need to move next through the list
                    int i = Next(hull.GetConvexHullPoints(), index);

                    pendingTanPoint = hull.GetConvexHullPoints()[i];
                    pendingSlope = CalculateSlope(pendingTanPoint, TLine.GetRightPoint());
                }
                else
                {
                    //TangentRightPoint will move to the next point in a CLOCKWISE direction
                    float rightPointX = TLine.GetRightPoint().X;
                    index = hull.GetConvexHullPoints().FindIndex(p => p.X == rightPointX);

                    //Start at the index, so to go CLOCKWISE we need to move next through the list
                    int i = Next(hull.GetConvexHullPoints(), index);

                    pendingTanPoint = hull.GetConvexHullPoints()[i];
                    pendingSlope = CalculateSlope(TLine.GetLeftPoint(), pendingTanPoint);
                }

                
                if (pendingSlope > slope)
                {
                    //That's good, so now we can change the tangent line
                    if (TangentType == LOWER_TANGENT)
                    {
                        TLine.SetLeftPoint(pendingTanPoint);
                    }
                    else
                    {
                        TLine.SetRightPoint(pendingTanPoint);
                    }
                    //And also update the slope
                    slope = pendingSlope;
                    //And the slope changed
                    slopeChanged = true;
                    slopeChangedCount++;
                }
                else
                {
                    slopeChanged = false;
                }

            } while (slopeChanged);
        }

        public ConvexHull MergeConvexHulls(ConvexHull hullLeft, ConvexHull hullRight, TangentLine upTan, TangentLine lowTan)
        {
            //Start at the upTan right point using the hullRight first because the upTan Right Point will be in the hullRight
            float upTanRightPointX = upTan.GetRightPoint().X;
            int index = hullRight.GetConvexHullPoints().FindIndex(p => p.X == upTanRightPointX);

            //Make a new convexHull that will contain the merged hulls
            ConvexHull mergedHull = new ConvexHull(upTan.GetRightPoint());

            //Move clockwise until we get to lowTan right point
            PointF nextPoint;
            bool sameIndexWasReturned = false;

            //If the starting point isn't already the lowertangent right point, then do the loop
            if (upTan.GetRightPoint().X != lowTan.GetRightPoint().X)
            {
                do
                {
                    int i = Next(hullRight.GetConvexHullPoints(), index, ref sameIndexWasReturned);
                    nextPoint = hullRight.GetConvexHullPoints()[i];

                    //Add the nextPoint into the mergedHull
                    if (!sameIndexWasReturned)
                    {
                        mergedHull.AddPoint(nextPoint);
                    }

                    //Update index
                    index = i;

                } while (nextPoint.X != lowTan.GetRightPoint().X);
            }

            //If we are right here, that means that the last point just added, was the Lower Tangent Right Point.
            //Now we want to add the Lower Tangent Left Point, and start iterating up CLOCKWISE the leftHull until we reach the point that is equal to the Upper Tangent Left Point.

            nextPoint = lowTan.GetLeftPoint();
            mergedHull.AddPoint(nextPoint);

            float lowTanLeftPointX = lowTan.GetLeftPoint().X;
            index = hullLeft.GetConvexHullPoints().FindIndex(p => p.X == lowTanLeftPointX);

            //If the lowertanget left point isn't already the upper tangent left point, then do the loop.
            if (lowTan.GetLeftPoint().X != upTan.GetLeftPoint().X)
            {
                sameIndexWasReturned = false;
                do
                {
                    int i = Next(hullLeft.GetConvexHullPoints(), index, ref sameIndexWasReturned);
                    nextPoint = hullLeft.GetConvexHullPoints()[i];

                    //Add the nextPoint into the mergedHull
                    if (!sameIndexWasReturned)
                    {
                        mergedHull.AddPoint(nextPoint);
                    }

                    //Update index
                    index = i;

                } while (nextPoint.X != upTan.GetLeftPoint().X);
            }

            //Once we reach here that means that last point added was the Upper Left Tangent Point, which is the last one we need to add for a complete hull.
            return mergedHull;
        }

        public ConvexHull FindConvexHull(List<PointF> points)
        {
            //Base Case 
            int n = points.Count;
            if(n == 1)
            {
                return new ConvexHull(points);
            }

            //Recursion
            ConvexHull chLeft = null;
            ConvexHull chRight = null;
            if(n > 1)
            {
                int mid = (int) Math.Ceiling((double)points.Count / 2);

                List<PointF> lowerHalf = GetPointsWithinRange(0, mid, points);
                List<PointF> upperHalf = GetPointsWithinRange(mid, n, points);

                chLeft = FindConvexHull(lowerHalf);
                chRight = FindConvexHull(upperHalf);

            }

            //Make Tangent Lines

            //Rightmost X value of the Left Hull
            PointF tanPointLeft = GetPointFromHull("RIGHTMOST", chLeft);
            //Leftmost X value of the Right Hull
            PointF tanPointRight = GetPointFromHull("LEFTMOST", chRight);

            TangentLine upperTangent = new TangentLine(tanPointLeft, tanPointRight);
            TangentLine lowerTangent = new TangentLine(tanPointLeft, tanPointRight);


            //Record the slope of the tangent lines
            double slope = CalculateSlope(upperTangent.GetLeftPoint(), upperTangent.GetRightPoint());
            bool thereCouldBeMoreChanges = false;
            //bool slopeChanged = false;

            //Major Loop (CONDITION) for UPPER TANGENT LINE
            do
            {
                int slopeChangedCount = 0;

                //UpperTanLeft Loop
                CounterClockwiseTangentMove(ref chLeft, ref upperTangent, ref slope, ref slopeChangedCount, "UPPER_TANGENT");

                //UpperTanRight Loop
                ClockwiseTangentMove(ref chRight, ref upperTangent, ref slope, ref slopeChangedCount, "UPPER_TANGENT");

                thereCouldBeMoreChanges = (slopeChangedCount > 0) ? true : false;

            } while (thereCouldBeMoreChanges);

            //Major Loop (CONDITION) for LOWER TANGENT LINE
            slope = CalculateSlope(lowerTangent.GetLeftPoint(), lowerTangent.GetRightPoint());
            do
            {
                int slopeChangedCount = 0;

                //LowerTanLeft Loop
                ClockwiseTangentMove(ref chLeft, ref lowerTangent, ref slope, ref slopeChangedCount, "LOWER_TANGENT");

                //LowerTanRight Loop
                CounterClockwiseTangentMove(ref chRight, ref lowerTangent, ref slope, ref slopeChangedCount, "LOWER_TANGENT");

                thereCouldBeMoreChanges = (slopeChangedCount > 0) ? true : false;

            } while (thereCouldBeMoreChanges);


            return MergeConvexHulls(chLeft, chRight, upperTangent, lowerTangent);
        }
    }

    class ConvexHull
    {
        //Member Variables
        private List<PointF> convexHullPoints;


        //Constructor #1
        public ConvexHull(List<PointF> list)
        {
            convexHullPoints = list;
        }

        //Constructor #2
        public ConvexHull(PointF point)
        {
            List<PointF> list = new List<PointF>();
            list.Add(point);

            convexHullPoints = list;
        }

       
        //Helper Methods

        //Get points
        public List<PointF> GetConvexHullPoints()
        {
            return convexHullPoints;
        }
        
        //Get point
        public PointF At(int i)
        {
            return convexHullPoints[i];
        }

        //Get First Point
        public PointF First()
        {
            return convexHullPoints[0];
        }

         //Get First Point
        public PointF Last()
        {
            return convexHullPoints[convexHullPoints.Count -1];
        }

        //Add point
        public void AddPoint(PointF p)
        {
            convexHullPoints.Add(p);
        }

        //Get number of points
        public int GetSize()
        {
            return convexHullPoints.Count;
        }

        //Go to next point
        //Go to previous point.
 



    }

    class TangentLine
    {
        //Member Variables
        private PointF leftPoint;
        private PointF rightPoint;

        //Constructor 1
        public TangentLine(PointF p1, PointF p2)
        {
            leftPoint = p1;
            rightPoint = p2;
        }

        public PointF GetLeftPoint()
        {
            return leftPoint;
        }

        public PointF GetRightPoint()
        {
            return rightPoint;
        }

        public void SetLeftPoint(PointF p)
        {
            leftPoint = p;
        }

        public void SetRightPoint(PointF p)
        {
            rightPoint = p;
        }




    }

}
