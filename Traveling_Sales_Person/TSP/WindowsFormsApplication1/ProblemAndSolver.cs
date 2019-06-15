using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TSP
{

    class ProblemAndSolver
    {
        public class NodeState
        {
            //private members
            private double[,] matrix;
            private double lowerBound;
            private ArrayList route;
            private int lastCityInRouteIndex;
            private List<bool> citiesVisited;


            //Constructor
            public NodeState(double[,] matrix, double lowerBound, ArrayList route, int lastCityInRouteIndex, List<bool> citiesVisited)
            {
                this.matrix = matrix;
                this.lowerBound = lowerBound;
                this.route = route;
                this.lastCityInRouteIndex = lastCityInRouteIndex;
                this.citiesVisited = citiesVisited;
            }

            //Methods
            public double[,] GetMatrix()
            {
                return this.matrix;
            }

            public double GetLowerBound()
            {
                return this.lowerBound;
            }

            public ArrayList GetRoute()
            {
                return this.route;
            }

            public int GetLastCityInRouteIndex()
            {
                return this.lastCityInRouteIndex;
            }

            public List<bool> GetCitiesVisited()
            {
                return this.citiesVisited;
            }

            public bool AlreadyVisited(int cityIndex)
            {
                return this.citiesVisited[cityIndex];
            }

            public bool HasVisitedAllCities()
            {
                int cityCount = this.citiesVisited.Count;
                for(int i = 0; i < cityCount; i++)
                {
                    if(citiesVisited[i] != true)
                    {
                        return false;
                    }
                }

                return true;
            }

            public void PrintMatrix(string title, int cityCount)
            {
                string strToPrint = title + ": \r\n";
                for (int j = 0; j < cityCount; j++)
                {
                    for (int i = 0; i < cityCount; i++)
                    {
                        if (this.matrix[i, j] == Double.PositiveInfinity)
                        {
                            strToPrint += "+ " + "   "; //Three extra spaces
                        }
                        else
                        {
                            strToPrint += matrix[i, j].ToString() + " ";
                            if (matrix[i, j] < 10)
                            {
                                strToPrint += "   "; //Three extra spaces
                            }
                            if (matrix[i, j] >= 10 && matrix[i, j] < 100)
                            {
                                strToPrint += "  "; //two extra space
                            }
                            if (matrix[i, j] >= 100 && matrix[i, j] < 1000)
                            {
                                strToPrint += " "; //one extra space
                            }
                        }
                    }
                    strToPrint += "\r\n";
                }

                strToPrint += "\r\nLower Bound: " + this.lowerBound.ToString() + "\r\n";
                Console.WriteLine(strToPrint);
            }

            public void ReduceMatrixAndUpdateLowerBound(int cityCount)
            {
                double lowestCost = Double.PositiveInfinity;
                //Reduce the ROWS
                for (int j = 0; j < cityCount; j++)
                {
                    for (int i = 0; i < cityCount; i++)
                    {
                        if (this.matrix[i, j] < lowestCost)
                        {
                            lowestCost = this.matrix[i, j];
                        }
                    }
                    //Gone through one row at this point
                    if (lowestCost != 0 && lowestCost != Double.PositiveInfinity)
                    {
                        //Add to lowerbound
                        this.lowerBound += lowestCost;
                        //Subtract from row values
                        for (int i = 0; i < cityCount; i++)
                        {
                            if (matrix[i, j] != Double.PositiveInfinity)
                            {
                                matrix[i, j] -= lowestCost;
                            }
                        }
                    }
                    //After all the subtracting is done in one row, we need to reset the lowest cost for the next row
                    lowestCost = Double.PositiveInfinity;
                }

                //Reduce the COLUMNS
                for (int i = 0; i < cityCount; i++)
                {
                    for (int j = 0; j < cityCount; j++)
                    {
                        if (this.matrix[i, j] < lowestCost)
                        {
                            lowestCost = this.matrix[i, j];
                        }
                    }
                    //Gone through one row at this point
                    if (lowestCost != 0 && lowestCost != Double.PositiveInfinity)
                    {
                        //Add to lowerbound
                        this.lowerBound += lowestCost;
                        //Subtract from row values
                        for (int j = 0; j < cityCount; j++)
                        {
                            if (matrix[i, j] != Double.PositiveInfinity)
                            {
                                matrix[i, j] -= lowestCost;
                            }
                        }
                    }
                    //After all the subtracting is done in one row, we need to reset the lowest cost for the next row
                    lowestCost = Double.PositiveInfinity;
                }
            }

        }

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf;

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;
        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;
        public const int TIME = 1;
        public const int COUNT = 2;

        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1;
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time * 1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue, 1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit * 1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width = g.VisibleClipBounds.Width - 45F;
            float height = g.VisibleClipBounds.Height - 45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count - 1)
                        g.DrawString(" " + index + "(" + c.costToGetTo(bssf.Route[index + 1] as City) + ")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else
                        g.DrawString(" " + index + "(" + c.costToGetTo(bssf.Route[0] as City) + ")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D;
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count = 0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];
            Stopwatch timer = new Stopwatch();
            timer.Start();
            //Obtain a BSSF by using a GREEDY approach.
            int startCity = 0;
            bssf = new TSPSolution(GenerateInitialBSSF(ref startCity));
            //Make an initial Matrix
            int cityCount = Cities.Length;
            double[,] initialMatrix = GetInitialMatrix(cityCount);
            //Put start city in list
            ArrayList startingRoute = new ArrayList();
            startingRoute.Add(Cities[startCity]);
            //Make Start node
            int lowerBound = 0;
            List<bool> citiesVisited = InitializeCitiesVisitedList(startCity, cityCount);
            NodeState startNode = new NodeState(initialMatrix, lowerBound, startingRoute, startCity, citiesVisited);
            startNode.ReduceMatrixAndUpdateLowerBound(cityCount);
            Console.WriteLine("Initial BSSF: " + bssf.costOfRoute().ToString());

            List<NodeState> priorityQueue = new List<NodeState>();
            priorityQueue.Add(null);
            priorityQueue.Add(startNode);

            //Now we are ready to execute the branch and bound algorithm
            int MaxNodeStateAmount = 1;
            int BSSFupdates = 0;
            int TotalNodeStates = 1;
            int TotalNodeStatesPrunned = 0;
            BranchAndBound(ref priorityQueue, ref MaxNodeStateAmount, ref BSSFupdates, ref TotalNodeStates, ref TotalNodeStatesPrunned, startCity, ref timer);

            timer.Stop();

            Console.WriteLine("Max NodeState Amount: " + MaxNodeStateAmount.ToString());
            Console.WriteLine("Total Node States: " + TotalNodeStates.ToString());
            Console.WriteLine("Total Node States Prunned: " + TotalNodeStatesPrunned.ToString() + "\r\n");
            results[COST] = bssf.costOfRoute().ToString();
            results[TIME] = timer.Elapsed.ToString(); 
            results[COUNT] = BSSFupdates.ToString();

            return results;
        }

        public ArrayList GenerateInitialBSSF(ref int startCity)
        {
            ArrayList route = new ArrayList();
            int cityCount = Cities.Length;
            bool routeCreated = false;


            while (!routeCreated && startCity < cityCount)
            {
                //Try a new city
                routeCreated = GenerateRoute(ref route, ref startCity);
                if (!routeCreated)
                {
                    startCity++;
                    route.Clear();
                }
            }

            //If we get to this point and no route was created from the greedy approach, just use the default random one
            if (!routeCreated)
            {
                Console.WriteLine("BSSF Initial used RANDOMIZER \r\n");
                route.Clear();
                int i, swap, temp, count = 0;
                string[] results = new string[3];
                int[] perm = new int[Cities.Length];
                Random rnd = new Random();

                do
                {
                    for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                        perm[i] = i;
                    for (i = 0; i < perm.Length; i++)
                    {
                        swap = i;
                        while (swap == i)
                            swap = rnd.Next(0, Cities.Length);
                        temp = perm[i];
                        perm[i] = perm[swap];
                        perm[swap] = temp;
                    }
                    route.Clear();
                    for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                    {
                        route.Add(Cities[perm[i]]);
                    }
                    bssf = new TSPSolution(route);
                    count++;
                } while (costOfBssf() == double.PositiveInfinity);

                return route;

            }
            else
            {
                return route;
            }
        }

        public bool GenerateRoute(ref ArrayList route, ref int startCity)
        {
            List<int> remainingCities = new List<int>();
            City currentCity = Cities[startCity];
            route.Add(currentCity);

            //Make a queue to help keep track of remaining cities
            //Remember to not add city 0
            int cityCount = Cities.Length;
            for (int i = 0; i < cityCount; i++) {
                if (i != startCity) {
                    remainingCities.Add(i);
                }
            }

            //Initialixe helper variables
            City neighborCity = null;
            int neighborCityIndex = 0;
            double neighborCost = Double.PositiveInfinity;
            double cost = 0;

            while (remainingCities.Count != 0)
            {
                //For each remaining city
                int size = remainingCities.Count;
                for (int i = 0; i < size; i++)
                {
                    int remainingCity = remainingCities[i];
                    cost = currentCity.costToGetTo(Cities[remainingCity]);
                    if (cost < neighborCost)
                    {
                        neighborCity = Cities[remainingCity];
                        neighborCost = cost;
                        neighborCityIndex = remainingCity;
                    }
                }

                //If the cost from the one city to the next comes out to be infinite then this route failed, and we have to start over.
                if (cost == Double.PositiveInfinity)
                {
                    return false;
                }

                //As it ends, we will have the closest neighbor to the current city
                route.Add(neighborCity);
                remainingCities.Remove(neighborCityIndex);
                currentCity = neighborCity;
                neighborCost = Double.PositiveInfinity;
            }

            //Verify that the last city in the route can make it back to the first
            City lastCity = (City)route[route.Count - 1];
            City beginCity = Cities[startCity];
            if (lastCity.costToGetTo(beginCity) == Double.PositiveInfinity)
            {
                return false;
            }

            //If we make it through, we found a route
            return true;
        }

        public double[,] GetInitialMatrix(int cityCount)
        {
            double[,] initialMatrix = new double[cityCount, cityCount];

            for (int i = 0; i < cityCount; i++) {
                for (int j = 0; j < cityCount; j++)
                {
                    if (i == j)
                    {
                        initialMatrix[i, j] = Double.PositiveInfinity;
                    }
                    else
                    {
                        initialMatrix[i, j] = Cities[j].costToGetTo(Cities[i]);
                    }

                }
            }

            return initialMatrix;
        }

        public List<bool> InitializeCitiesVisitedList(int startCity, int cityCount)
        {
            List<bool> citiesVisited = new List<bool>();
            for (int i = 0; i < cityCount; i++)
            {
                if (i == startCity)
                {
                    citiesVisited.Add(true);
                }
                else
                {
                    citiesVisited.Add(false);
                }

            }
            return citiesVisited;
        }

        public void BranchAndBound(ref List<NodeState> queue, ref int MaxNodeStateAmount, ref int BSSFupdates, ref int TotalNodeStates, ref int TotalNodeStatesPrunned, int startCity, ref Stopwatch timer)
        {

            int empty = 1; //Empty is 1, because the priority queue is a MIN HEAP. Index 0 is set to null.
            while (queue.Count != empty && timer.ElapsedMilliseconds < time_limit)
            {
 
                GenerateChildren(ref queue, ref MaxNodeStateAmount, ref BSSFupdates, ref TotalNodeStates, ref TotalNodeStatesPrunned, startCity);
            }

            //Now we have reached the end of the branching, prune whatever else is on the queue
            int remainingStates = queue.Count - 1;
            TotalNodeStatesPrunned += remainingStates;

        }

        public void GenerateChildren(ref List<NodeState> queue, ref int MaxNodeStateAmount, ref int BSSFupdates, ref int TotalNodeStates, ref int TotalNodeStatesPrunned, int startCity)
        {
            int cityCount = Cities.Length;
            //Get the first node in the priority
            NodeState currentNodeState = DeletePriorityNode(ref queue);
            //If the currentNodeState should be prunned
            if(currentNodeState.GetLowerBound() >= bssf.costOfRoute())
            {
                currentNodeState = PruneTheQueue(ref queue, currentNodeState, ref TotalNodeStatesPrunned);
            }
            //Generate a child for each city the currentNodeState can go to.
            for (int i = 0; i < cityCount; i++)
            {
                if (!currentNodeState.AlreadyVisited(i))
                {
                    //Make a child
                    NodeState child = MakeChild(ref currentNodeState, i);
                    child.ReduceMatrixAndUpdateLowerBound(cityCount);
                    // Add to node states because a child was generated
                    TotalNodeStates++; 

                    //If LB is less BSSF then add to queue, TNS++
                    if (child.GetLowerBound() < bssf.costOfRoute())
                    {
                        //Before adding to the queue, see if you have a new BSSF
                        if (BetterBSSFExists(ref child, startCity))
                        {
                            bssf = new TSPSolution(child.GetRoute());
                            BSSFupdates++;

                        }
                        else
                        {
                            //add node to the queue
                            if(!child.HasVisitedAllCities())
                            {
                                //Add
                                AddNodeToPriorityQueue(ref queue, child);
                            }
                        }
                    }
                    else
                    {
                        //Prune. We don't need to even visit it.
                        TotalNodeStatesPrunned++;
                    }
                }
            }

            //Check the MaxNodeStates
            CheckQueueSize(ref MaxNodeStateAmount, queue.Count - 1);
           
        }

        public NodeState DeletePriorityNode(ref List<NodeState> queue)
        {
            //Get the first node on the heap which will be arranged at the highest priority
            NodeState nodeFirst = queue[1];

            //Get the last node and stick it at the top of the min heap
            int queueSize = queue.Count - 1;
            NodeState last = queue[queueSize];

            //Now sift that last node down into the proper spot in the heap
            SiftDown(ref queue, last, 1, queueSize);

            //Return the original first node from the priority queue
            return nodeFirst;

        }

        public NodeState PruneTheQueue(ref List<NodeState> queue, NodeState currentNodeState, ref int TotalNodeStatesPrunned)
        {
            while(currentNodeState.GetLowerBound() >= bssf.costOfRoute() && queue.Count != 1)
            {
                currentNodeState = DeletePriorityNode(ref queue);
                TotalNodeStatesPrunned++;
            }

            return currentNodeState;
        }

        public void SiftDown(ref List<NodeState> queue, NodeState last, int i, int queueSize)
        {
            //put node last in position 1 of the queue and let it sift down.
            int min = SmallestChild(ref queue, i, queueSize);
            while ((min != 0) && (queue[min].GetLowerBound() < last.GetLowerBound()))
            {
                queue[i] = queue[min];
                i = min;
                min = SmallestChild(ref queue, i, queueSize);

            }

            if (queue[i] != last)
            {
                queue[i] = last;
            }

            //Remove last node in the heap because we must have one less
            queue.RemoveAt(queueSize);
        }

        public int SmallestChild(ref List<NodeState> queue, int i, int queueSize)
        {
            //Return index of the smallest child of queue[i]
            if ((2 * i) > queueSize)
            {
                //There are no children
                return 0;
            }
            else
            {
                //Get a child, we know there is at least a LEFT CHILD

                //Check if there is a RIGHT CHILD
                if ((2 * i) + 1 > queueSize)
                {
                    //return left child
                    return 2 * i;
                }
                //if the two children are equal
                if (queue[2 * i].GetLowerBound() == queue[(2 * i) + 1].GetLowerBound())
                {
                    //return the right child
                    return (2 * i) + 1;
                }
                //If the left child is greater than the right child
                if (queue[2 * i].GetLowerBound() > queue[(2 * i) + 1].GetLowerBound())
                {
                    //return the right child
                    return (2 * i) + 1;
                }
                else
                {
                    //return left child
                    return 2 * i;
                }

            }
        }

        public void AddNodeToPriorityQueue(ref List<NodeState> queue, NodeState child)
        {
            queue.Add(child);
            int lastIndex = queue.Count - 1;
            SiftUp(ref queue, lastIndex);
        }

        public void SiftUp(ref List<NodeState> queue, int index)
        {
            //Get parent index
            int startOfQueue = 1;
            int parentIndex = index / 2;
            if(index != startOfQueue)
            {
                if(queue[index].GetLowerBound() < queue[parentIndex].GetLowerBound())
                {
                    //Swap
                    NodeState temp = queue[index];
                    queue[index] = queue[parentIndex];
                    queue[parentIndex] = temp;

                    SiftUp(ref queue, parentIndex);
                }
            }
        }

        public NodeState MakeChild(ref NodeState parentNode, int i)
        {
            //FROM and TO city indices
            int cityCount = Cities.Length;
            int fromCity = parentNode.GetLastCityInRouteIndex();
            int toCity = i;

            //Make a child matrix from parent
            double[,] copyMatrix = CopyMatrix(parentNode.GetMatrix(), cityCount);
            double[,] childMatrix = MakeChildMatrix(copyMatrix, fromCity, toCity);

            //Inherit and add to lower bound for the child
            double childLB = parentNode.GetLowerBound() + parentNode.GetMatrix()[toCity, fromCity];

            //Inherit and add to route for the child
            ArrayList childRoute = (ArrayList) parentNode.GetRoute().Clone();
            childRoute.Add(Cities[toCity]);

            //Inherit the cities already visited by the parent and add the city the child is going to be in.
            List<bool> childCitiesVisited = CopyCitiesVisited(parentNode.GetCitiesVisited());
            childCitiesVisited[toCity] = true;

            return new NodeState(childMatrix, childLB, childRoute, toCity, childCitiesVisited);
        }

        public double[,] CopyMatrix(double[,] matrix, int cityCount)
        {
            double[,] matrixCopy = new double[cityCount,cityCount];
            
            for (int i = 0; i < cityCount; i++){
                for (int j = 0; j < cityCount; j++){

                    matrixCopy[i, j] = (double) matrix[i, j];
                }
            }

            return matrixCopy;
        }
        
        public List<bool> CopyCitiesVisited(List<bool> list)
        {
            List<bool> copyCitiesVisited = new List<bool>();
            for(int i = 0; i < list.Count; i++)
            {
                bool temp = list[i];
                copyCitiesVisited.Add(temp);
            }

            return copyCitiesVisited;
        }

        public double[,] MakeChildMatrix(double[,] parentMatrix, int fromCity, int toCity)
        {
            int cityCount = Cities.Length;
            //FROM is J
            //TO is I
            //Infinity out the correct spots in the parentMatrix

            //Infinity out the row from
            for (int i = 0; i < cityCount; i++)
            {
                parentMatrix[i, fromCity] = Double.PositiveInfinity;
            }
            //Infinity out the column to
            for (int j = 0; j < cityCount; j++)
            {
                parentMatrix[toCity, j] = Double.PositiveInfinity;
            }
            //Infinity out the backedge
            parentMatrix[fromCity, toCity] = Double.PositiveInfinity;

            return parentMatrix;
        }

        public bool BetterBSSFExists(ref NodeState node, int startCity)
        {
            if(node.HasVisitedAllCities())
            {
                //check if it can go back to it's first city
                int currentCity = node.GetLastCityInRouteIndex();
                double[,] nodeMatrix = node.GetMatrix();
                if(nodeMatrix[startCity,currentCity] != Double.PositiveInfinity)
                {
                    //We can go back.
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        public void CheckQueueSize(ref int MaxNodeStateAmount, int nodesInQueue)
        {
            if (nodesInQueue > MaxNodeStateAmount)
            {
                MaxNodeStateAmount = nodesInQueue;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for a greedy solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for your advanced solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
    }

}
