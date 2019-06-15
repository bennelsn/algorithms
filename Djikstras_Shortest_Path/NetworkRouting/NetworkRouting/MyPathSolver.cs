using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;

namespace NetworkRouting
{
    class MyPathSolver
    {
        System.Drawing.Graphics g;
        System.Windows.Forms.PictureBox pictureBox;
        System.Windows.Forms.TextBox pathCostBox;
        int infinity = 10000000;
        int nil = -1;

        public MyPathSolver(System.Drawing.Graphics g, System.Windows.Forms.PictureBox pictureBox, System.Windows.Forms.TextBox pathCostBox)
        {
            this.g = g;
            this.pictureBox = pictureBox;
            this.pathCostBox = pathCostBox;
        }

        public void SolveArray(List<System.Drawing.PointF> nodeList, List<HashSet<int>> edgeList, int startNode, int stopNode)
        {
            //The nodeList is a list of points on a coordinate system where the nodes are located
            //The edgeList is a list that for each index (the index number corresponds to the node index number) has a hashlist of 3 nodes. The 3 nodes are connected to the node. 

            //Initialize 
            List<double> distanceList = MakeDistanceList(nodeList);
            List<int> prevList = MakePrevList(nodeList);
            distanceList[startNode] = 0;
            List<int> queue = MakeArrayPriorityQueue(startNode, nodeList.Count);
      
            while (queue.Count != 0)
            {
                int currentNode = DeleteMinArray(ref queue);
                HashSet<int> edges = edgeList[currentNode];

                
                foreach (int edgeNode in edges)
                {
                    double distance = CalculateDistance(currentNode, edgeNode, nodeList);
                    if (distanceList[edgeNode] > (distanceList[currentNode] + distance))
                    {
                        distanceList[edgeNode] = distanceList[currentNode] + distance;
                        prevList[edgeNode] = currentNode;
                    }
                    else
                    {
                        continue;
                    }

                    if (queue.Count != 0)
                    {
                        DecreaseKeyArray(queue, distanceList);
                    }

                }

                
            }

            DrawPath(startNode, stopNode, nodeList, prevList);

        }

        
        public void SolveHeap(List<System.Drawing.PointF> nodeList, List<HashSet<int>> edgeList, int startNode, int stopNode)
        {
            //The nodeList is a list of points on a coordinate system where the nodes are located
            //The edgeList is a list that for each index (the index number corresponds to the node index number) has a hashlist of 3 nodes. The 3 nodes are connected to the node. 

            //Initialize 
            List<double> distanceList = MakeDistanceList(nodeList);
            List<int> prevList = MakePrevList(nodeList);
            distanceList[startNode] = 0;

            //Make a list to keep track of spot or location of where each node is in what index of the queue (heap)
            List<int> spotList = MakeSpotList(nodeList.Count);

            //MAKE HEAP
            
            List<int> queue = MakeHeapPriorityQueue(startNode, nodeList.Count, ref spotList);

            int heapSize = nodeList.Count;
            while (heapSize != 0)
            {
                //DELETE MIN FROM HEAP
                int currentNode = DeleteMinHeap(ref queue, ref heapSize, ref spotList, distanceList);
                HashSet<int> edges = edgeList[currentNode];


                foreach (int edgeNode in edges)
                {
                    double distance = CalculateDistance(currentNode, edgeNode, nodeList);
                    if (distanceList[edgeNode] > (distanceList[currentNode] + distance))
                    {
                        distanceList[edgeNode] = distanceList[currentNode] + distance;
                        prevList[edgeNode] = currentNode;
                        DecreaseKeyHeap(ref queue, edgeNode, ref spotList, distanceList);
                    }
                }
            }

            DrawPath(startNode, stopNode, nodeList, prevList);
            
        }

        public List<double> MakeDistanceList(List<System.Drawing.PointF> nodeList)
        {
            List<double> distanceList = new List<double>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                distanceList.Add(infinity);
            }

            return distanceList;
        }

        public List<int> MakePrevList(List<System.Drawing.PointF> nodeList)
        {
            List<int> prevList = new List<int>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                prevList.Add(nil);
            }

            return prevList;
        }

        public List<int> MakeSpotList(int nodeCount)
        {
            List<int> spotList = new List<int>();

            for(int i =0; i < nodeCount; i++)
            {
                spotList.Add(0);
            }

            return spotList;
        }



        public List<int> MakeArrayPriorityQueue(int startNode, int nodeCount)
        {
            //ARRAY for just making the initial queue
            List<int> pQueue = new List<int>();
            pQueue.Add(startNode);
            for (int i = 0; i < nodeCount; i++)
            {
                if (startNode != i)
                {
                    pQueue.Add(i);
                }
                else
                {
                    i++;
                    pQueue.Add(i);
                }
            }
            return pQueue;
        }

        public List<int> MakeHeapPriorityQueue(int startNode, int nodeCount, ref List<int> spotList)
        {
            //HEAP for just making the initial queue
            List<int> heapQueue = new List<int>();
            heapQueue.Add(nil);
            heapQueue.Add(startNode);
            spotList[startNode] = 1;

            int j = 0;
            for (int i = 0; i < nodeCount; i++)
            {
                if (startNode != i)
                {
                    j++;
                    heapQueue.Add(i);
                    spotList[i] = j + 1;
                }
                else
                {
                    if (i == spotList.Count - 1)
                    {
                        //The last node is the start node in this special case
                        spotList[i] = 1;
                    }
                    else
                    {
                        i++;
                        j++;
                        heapQueue.Add(i);
                        spotList[i] = j + 1;
                    }
                }
            }
            return heapQueue;
        }

        public int DeleteMinArray(ref List<int> queue)
        {
            int pNode = queue[0];
            List<int> updatedQueue = new List<int>();

            //Essentially chop off the first node which is the priority
            for (int i = 1; i < queue.Count; i++)
            {
                //Add the rest of the nodes back onto the queue.
                updatedQueue.Add(queue[i]);
            }

            queue = updatedQueue;
            return pNode;

        }

        public int DeleteMinHeap(ref List<int> queue, ref int heapSize, ref List<int> spotList, List<double> distanceList)
        {
            //Get the first node on the heap which will be arranged at the highest priority
            int nodeFirst = queue[1];
            spotList[nodeFirst] = nil;

            int nodeLast = queue[heapSize];
            SiftDown(ref queue, ref spotList, nodeLast, 1, heapSize, distanceList);
            queue[heapSize] = nil;
            heapSize--;

            return nodeFirst;
        }

        public void SiftDown(ref List<int> queue, ref List<int> spotList, int last, int i, int heapSize, List<double> distanceList)
        {
            //put node last in position 1 of the queue and let it sift down.
            int min = SmallestChild(queue, i, heapSize, spotList, distanceList);
            while( (min != 0) && (distanceList[queue[min]] < distanceList[last]) )
            {
                queue[i] = queue[min];
                spotList[queue[i]] = i;
                i = min;
                min = SmallestChild(queue, i, heapSize, spotList, distanceList);

            }

            if( queue[i] != last )
            {
                queue[i] = last;
                spotList[last] = i;
            }
        }

        public int SmallestChild(List<int> queue, int i, int heapSize, List<int> spotList, List<double> distanceList)
        {
            //Return index of teh smallest child of queue[i]
            if((2*i) > heapSize)
            {
                //There are no children
                return 0;
            }
            else
            {
                //Get a child

                //if right child is nil (not really there)
                if(queue[(2*i) + 1] == nil)
                {
                    //return left child
                    return spotList[queue[2 * i]];
                }
                //if the two children are equal
                if(distanceList[queue[2*i]] == distanceList[queue[(2*i) + 1]])
                {
                    //return the right child
                    return spotList[queue[(2 * i) + 1]];
                }
                //If the left child is greater than the right child
                if(distanceList[queue[2 * i]] > distanceList[queue[(2 * i) + 1]])
                {
                    //return the right child
                    return spotList[queue[(2 * i) + 1]];
                }
                else
                {
                    //return left child
                    return spotList[queue[2 * i]];
                }

            }

        }




        public double CalculateDistance(int start, int end, List<System.Drawing.PointF> nodeList)
        {
            PointF p1 = nodeList[start];
            PointF p2 = nodeList[end];

            //sqrt((X2 - X1)^2 + (Y2 - Y1)^2)
            double a = (p2.X - p1.X);
            a = Math.Pow(a, 2);

            double b = (p2.Y - p1.Y);
            b = Math.Pow(b, 2);

            return Math.Sqrt(a + b);

        }

        public double CalculateDistance(PointF p1, PointF p2)
        {
            //sqrt((X2 - X1)^2 + (Y2 - Y1)^2)
            double a = (p2.X - p1.X);
            a = Math.Pow(a, 2);

            double b = (p2.Y - p1.Y);
            b = Math.Pow(b, 2);

            return Math.Sqrt(a + b);

        }

        public PointF CalculateMidPoint(PointF p1, PointF p2)
        {
            //x1 + x2/2 , y1 + y2/2
            float x = (p1.X + p2.X) / 2.0f;
            float y = (p1.Y + p2.Y) / 2.0f;

            return new PointF(x, y);

        }

        public void DecreaseKeyArray(List<int> queue, List<double> distanceList)
        {
            int pNodeIndex = 0;
       
            double distance = distanceList[queue[0]];

            for (int i = 0; i < queue.Count; i++)
            {
                if(distanceList[queue[i]] < distance)
                {
                    //Go to front of queue
                    pNodeIndex = i;
                    distance = distanceList[queue[i]];
                }
            }
            int node = queue[pNodeIndex];
            queue[pNodeIndex] = queue[0];
            queue[0] = node;

        }

        public void DecreaseKeyHeap(ref List<int> queue, int edgeNode, ref List<int> spotList, List<double> distanceList)
        {
            //Bubble Up
            int i = spotList[edgeNode];
            int p = i / 2;
            int s = 0;

            while ( (i != 1) && (distanceList[queue[p]] > distanceList[queue[i]]) )
            {
                
                s = queue[i];
                queue[i] = queue[p];
                spotList[queue[i]] = i;
                queue[p] = s;
                spotList[s] = p;
                i = p;
                p = i / 2;
               
            }
        }

        public void DrawPath(int startNode, int stopNode, List<System.Drawing.PointF> nodeList,  List<int> prevList)
        {
            List<int> drawList = new List<int>();
            drawList.Add(stopNode);
            int value = stopNode;

            do
            {
                int lookup = prevList[value];
                if(lookup != nil)
                {
                    drawList.Add(lookup);
                }
                value = lookup;

            } while (value != nil);

            if(drawList[drawList.Count - 1] != startNode)
            {
                //Then we can't make a path
                pathCostBox.Text = "unreachable";
                
            }
            else
            {
                DrawFromNodeList(drawList, nodeList);
            }




        }

        public void DrawFromNodeList(List<int> key, List<System.Drawing.PointF> nodeList)
        {
            double totalDistance = 0;
            for (int i = 0; i < key.Count - 1; i++) 
            {
                //Get a point
                PointF p1 = nodeList[key[i]];
                PointF p2 = nodeList[key[i+1]];

                Pen pen = new Pen(Color.Red, 1.0f);
                g.DrawLine(pen, p1, p2);

                double distance = CalculateDistance(p1, p2);
                totalDistance += distance;
                int trunkedDistance = (int)distance;

                PointF midpoint = CalculateMidPoint(p1, p2);
                // Create font and brush.
                Font drawFont = new Font("Arial", 16);
                SolidBrush drawBrush = new SolidBrush(Color.Black);

                // Draw string to screen.
                g.DrawString(trunkedDistance.ToString(), drawFont, drawBrush, midpoint);

            }
            //Draw the last line
            //g.DrawLine(pen, ch.Last(), ch.First());
            pathCostBox.Text = totalDistance.ToString();




        }

        public void PrintList(string listName, List<int> list)
        {
            Console.Write(listName + ":\n\n");
            string build = "";
            if (list.Count == 0)
            {
                Console.Write(listName + " has nothing in it.");
            }
            else
            {

                for (int i = 0; i < list.Count; i++)
                {
                    string index = i.ToString();
                    int value = list[i];
                    build += ("Index: " + index + " = " + value.ToString() + "\n");

                }
            }
            build += "\n\n";
            Console.Write(build);
        }

        public void PrintList(string listName, List<double> list)
        {
            Console.Write(listName + ":\n\n");
            string build = "";
            if (list.Count == 0)
            {
                Console.Write(listName + " has nothing in it.");
            }
            else
            {

                for (int i = 0; i < list.Count; i++)
                {
                    string index = i.ToString();
                    double value = list[i];
                    build += ("Index: " + index + " = " + value.ToString() + "\n");

                }
            }
            build += "\n\n";
            Console.Write(build);
        }
    }
}
