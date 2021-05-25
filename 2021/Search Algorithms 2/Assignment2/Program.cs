using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment2 {
    class Program {
        static void Main(string[] args) {
            // Set up a simple text menu for choosing puzzle and settings
            Console.WriteLine("This program will solve the n-queens problem with different algorithms.");

            // Specify size of n
            Console.WriteLine("First, please input the value of n.");

            int n = 0;

            while (!int.TryParse(Console.ReadLine(), out n) || n <= 0) {
                Console.WriteLine("Please input a positive integer for n.");
            }

            // Specify search type/algorithm
            Console.WriteLine("Which algorithm would you like to solve the problem with?");
            Console.WriteLine("Type any of the following integers to make your selection.");
            Console.WriteLine("1 - Hill Climbing");
            Console.WriteLine("2 - Genetic Algorithm");

            int searchInt = 0;

            while (!int.TryParse(Console.ReadLine(), out searchInt) || searchInt < 1 || searchInt > 2) {
                Console.WriteLine("Please select one of the valid options, remember to only input the integer.");
            }

            // Convert searchInt into the proper string
            string searchType = "";

            switch (searchInt) {
                case 1:
                    searchType = "hc";
                    break;

                case 2:
                    searchType = "ga";
                    break;

            }

            // Generate the initial state
            Console.WriteLine("Generating an initial state...");
            PuzzleState initial;
            LinkedList<PuzzleState> solution = null;

            // Create a stopwatch to keep track of time
            Stopwatch watch = new Stopwatch();

            initial = Nqueens.Generate(n);

            Console.WriteLine("Solving for generated n-queens puzzle...");

            watch.Start();
            solution = Nqueens.SolvePuzzle(n, initial, searchType);
            watch.Stop();

            // Catch if no solution was found
            if (solution == null) {
                Console.WriteLine("Either puzzle is invalid, or solution could not be found.");

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                Environment.Exit(0);
            }

            Console.WriteLine("Solution found with {0} steps in {1} seconds!", solution.Length, watch.ElapsedMilliseconds / 1000.0);

            Console.WriteLine("Would you like a text file of the steps? (y/n)");
            char response;

            while (!char.TryParse(Console.ReadLine(), out response) || char.ToLower(response) != 'y' && char.ToLower(response) != 'n') {
                Console.WriteLine("Please input a valid option, only a single char y or n.");
            }

            if (char.ToLower(response) == 'y') {
                // Build the filename
                string filename;
                filename = "nqueens_";
                filename += n;
                filename += "_solution.txt";

                try {
                    // Find the max number of digits using n, for formatting purposes
                    //int digits = (int)Math.Floor(Math.Log10(n + 1) + 1);
                    string format = "{0} ";

                    StreamWriter sw = new StreamWriter(filename);
                    LinkedList<PuzzleState>.Node node = solution.First;

                    for (int i = 0; i < solution.Length; i++) {
                        for (int y = 0; y < node.data.Width; y++) {
                            for (int x = 0; x < node.data.Width; x++) {
                                sw.Write(format, node.data[x, y]);
                            }
                            sw.WriteLine();
                        }
                        sw.WriteLine();
                        node = node.next;
                    }

                    sw.Close();

                } catch (IOException e) {
                    Console.WriteLine("Something went wrong when writing to the file:");
                    Console.WriteLine(e);
                }

                Console.WriteLine("File written successfully.");
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    class Nqueens {
        int n;
        PuzzleState goal;

        public static LinkedList<PuzzleState> SolvePuzzle(int n, PuzzleState state, string searchType) {
            Nqueens queensPuzzle = new Nqueens(n);
            GraphSearch<PuzzleState> solver = new GraphSearch<PuzzleState>();
            if (searchType == "ga") {
                return solver.graphSearch(state, queensPuzzle.GetQueenScore, queensPuzzle.ComputeSuccessors, searchType, queensPuzzle.ComputeGeneticSuccessors, queensPuzzle.GenerateRandomInitial);
            } else {
                return solver.graphSearch(state, queensPuzzle.GetQueenScore, queensPuzzle.ComputeSuccessors, searchType);
            }
            
        }

        public static PuzzleState Generate(int n) {
            int[] board = new int[n * n];

            // Populate the initial state with 1 representing a queen
            // 0 represents an empty space
            // Initial state will always be all queens starting on the top row
            for (int i = 0; i < n * n; i++) {
                if (i < n) board[i] = 1;
                else board[i] = 0;
            }

            return new PuzzleState(board, n);
        }

        public Nqueens(int n) {
            this.n = n;
        }

        // TODO: Make new GetScore function, using new method:
        // Search each column from top to bottom, starting left to right
        // upon finding a queen, check the following directions.
        // Up-right, Right, Down-right
        // Add 1 for each queen found along those paths, then iterate to the next queen
        // This will give us the number of pairs of queens attacking each other
        // and since we are only searching in one direction, we do not need to worry
        // about duplicate counts.
        public int GetQueenScore(PuzzleState state) {
            int score = 0;

            for (int x = 0; x < n; x++) {
                for (int y = 0; y < n; y++) {
                    // Once a queen is found...
                    if (state[x,y] == 1) {
                        int checkX = x + 1;
                        int checkY = y - 1;
                        
                        // Search up-right direction
                        while (checkX < n && checkY >= 0) {
                            score += state[checkX, checkY];

                            checkX++;
                            checkY--;
                        }

                        checkX = x + 1;

                        // Search straight-right direction
                        while (checkX < n) {
                            score += state[checkX, y];

                            checkX++;
                        }

                        checkX = x + 1;
                        checkY = y + 1;

                        // Search down-right direction
                        while (checkX < n && checkY < n) {
                            score += state[checkX, checkY];

                            checkX++;
                            checkY++;
                        }
                    }
                }
            }

            return score;
        }


        // TODO: Attempt to fix this GetScore below (note that diagonals are counting twice with this method)
        // That may be an issue, or may not, but should prooobably try to fix it
        
        // Function used for ensuring no queens are attacking each other
        // Note that this does not check the columns since we are not allowing the queens
        // to change columns when moving
        public int GetScore(PuzzleState state) {
            int score = 0;
            
            // First we will check all rows for more than 1 queen
            for (int y = 0; y < n; y++) {
                int queenCount = 0;

                for (int x = 0; x < n; x++) {
                    // Since queens are 1 and empty spaces are 0, this will naturally give us the number of queens for this row
                    queenCount += state[x, y];
                }

                // If more than one queen has been found in this row, then the puzzle is not solved
                if (queenCount > 1) score += (int)Math.Pow(2, queenCount - 1);
                // If there's an empty row, this also contributes to not having a solution, so increase score
                if (queenCount == 0) score += 2;
            }

            // Now that the rows are complete, and no need to check columns...
            // Check diagonals
            for (int i = 0; i < n - 1; i++) {
                int queenCountXPos = 0;
                int queenCountYPos = 0;
                int queenCountXNeg = 0;
                int queenCountYNeg = 0;

                for (int j = 0; i + j < n; j++) {
                    queenCountXPos += state[i + j, j];
                    queenCountYPos += state[j, i + j];
                    queenCountXNeg += state[(n - 1) - (i + j), j];
                    queenCountYNeg += state[(n - 1) - j, i + j];
                }
                
                if (queenCountXPos > 1) score += queenCountXPos - 1;
                if (queenCountYPos > 1) score += queenCountYPos - 1;
                if (queenCountXNeg > 1) score += queenCountXNeg - 1;
                if (queenCountYNeg > 1) score += queenCountYNeg - 1;
            }

            // If both are checked and no queens are on same row/diagonal, then we have the solution
            return score;
        }

        public LinkedList<PuzzleState> ComputeSuccessors(PuzzleState state) {
            LinkedList<PuzzleState> ret = new LinkedList<PuzzleState>();
            PuzzleState alteredState;

            // Search all columns for each queen and move it to all possible spaces
            // All resulting boards will be added as children
            for (int x = 0; x < n; x++) {
                // First, find the queen "q" by iterating through the column
                int q;
                for (q = 0; q < n; q++) {
                    if (state[x, q] == 1) break;
                }

                for (int y = 0; y < n; y++) {
                    // If current coordinate is where the queen is, skip
                    if (y == q) continue;

                    alteredState = new PuzzleState(state);
                    alteredState.Swap(x, q, x, y);
                    ret.Append(alteredState);
                }
            }

            return ret;
        }

        // Genetic Algorithm specific functions
        public LinkedList<PuzzleState> ComputeGeneticSuccessors(LinkedList<PuzzleState> bestChildren) {
            LinkedList<PuzzleState> newChildren = new LinkedList<PuzzleState>();
            Random random = new Random();

            // For each child in the list, crossover with each unique pair
            // Starting from the front of the list and moving to the end
            LinkedList<PuzzleState>.Node currentChild = bestChildren.First;
            while (currentChild != null) {
                LinkedList<PuzzleState>.Node nextChild = currentChild.next;

                while (nextChild != null) {
                    PuzzleState child1 = new PuzzleState(currentChild.data);
                    PuzzleState child2 = new PuzzleState(nextChild.data);

                    // To crossover, overwrite the "right half" of the board to simulate crossing genes
                    for (int x = n/2; x < n; x++) {
                        for (int y = 0; y < n; y++) {
                            child1[x, y] = nextChild.data[x, y];
                            child2[x, y] = currentChild.data[x, y];
                        }
                    }

                    // Now mutate the children post-crossover
                    int column1 = random.Next(n);
                    int column2 = random.Next(n);

                    for (int i = 0; i < n; i++) {
                        child1[column1, i] = 0;
                        child2[column2, i] = 0;
                    }

                    child1[column1, random.Next(n)] = 1;
                    child2[column2, random.Next(n)] = 1;

                    // Add to new children list
                    newChildren.Append(child1);
                    newChildren.Append(child2);

                    nextChild = nextChild.next;
                }
                currentChild = currentChild.next;
            }

            // Lastly mutate all of the initial children once
            currentChild = bestChildren.First;

            while (currentChild != null) {
                PuzzleState newChild = new PuzzleState(currentChild.data);

                for (int j = 0; j < n/2; j++) {
                    int column = random.Next(n);

                    // Remove the old queen in this column
                    for (int i = 0; i < n; i++) {
                        newChild[column, i] = 0;
                    }

                    // Add new "mutated" queen in same column
                    newChild[column, random.Next(n)] = 1;
                }
                newChildren.Append(newChild);

                currentChild = currentChild.next;
            }

            // Return list of successors
            return newChildren;
        }

        public LinkedList<PuzzleState> GenerateRandomInitial() {
            LinkedList<PuzzleState> ret = new LinkedList<PuzzleState>();
            Random random = new Random();

            // We generate a random set of 16 boards as an initial population
            for (int i = 0; i < 16; i++) {
                PuzzleState newState = new PuzzleState(new int[n*n], n);
                // Place the new queens randomly
                for (int x = 0; x < n; x ++) {
                    newState[x, random.Next(n)] = 1;
                }
                ret.Append(newState);
            }

            return ret;
        }
    }

    // Graph Search class is for the sake of the previousStates list, as it is important for comparison to avoid dupes or find solutions in the case of bi-directional search
    class GraphSearch<T> {
        LinkedList<T> previousStates = new LinkedList<T>();

        public LinkedList<T> graphSearch(T data, Func<T, int> getScore, Func<T, LinkedList<T>> computeSuccessors, string searchType, Func<LinkedList<T>, LinkedList<T>> algorithmSpecificSuccessors=null, Func<LinkedList<T>> generateInitial=null, Func<T, Tuple<int, int>> generateChange=null, Func<Tuple<T, int, int, int>[], LinkedList<T>> applyBestChange=null) {
            Tree<T> root = new Tree<T>(data);

            // Hill Climbing
            if (searchType == "hc") {
                Tree<T> solution = HillClimbing(root, getScore, computeSuccessors);

                // If the solution was found then we return the proper steps for the solution
                if (solution != null) return SolutionSteps(solution);
            }

            // Genetic Algorithm
            if (searchType == "ga") {
                Tree<T> solution = GeneticAlgorithm(root, getScore, algorithmSpecificSuccessors, generateInitial);

                if (solution != null) return SolutionSteps(solution);
            }

            // Else, we've exhausted all states with no solution
            return null;
        }

        // Hill climbing function
        Tree<T> HillClimbing(Tree<T> data, Func<T, int> getScore, Func<T, LinkedList<T>> computeSuccessors) {
            int currentScore = getScore(data.value);
            Tree<T> currentState = data;
            // While the solution is not found (score > 0)
            while (currentScore > 0) {
                // If we are processing the same state again (it already has children)
                // then return null as we have hit a local max and the algorithm will get stuck otherwise
                // This will result in the user seeing that no solution was found
                if (!currentState.children.isEmpty()) {
                    return null;
                }

                // Find all possible moves
                currentState.AddChildren(computeSuccessors(currentState.value));

                // Check each move starting with the first
                LinkedList<Tree<T>>.Node child = currentState.children.First;
                while (child != null) {
                    // Find the new move's score
                    int childScore = getScore(child.data.value);

                    // If this move results in a better score (less queens attacking)
                    // Then this is the new node
                    if (childScore < currentScore) {
                        Console.WriteLine("childScore = " + childScore);
                        Console.WriteLine("currentScore = " + currentScore);

                        currentState = child.data;
                        currentScore = childScore;

                        // Now loop and repeat with the new node
                        break;
                    }
                    child = child.next;
                }
            }

            return currentState;
        }

        Tree<T> GeneticAlgorithm(Tree<T> data, Func<T, int> getScore, Func<LinkedList<T>, LinkedList<T>> computeSuccessors, Func<LinkedList<T>> generateInitial) {
            // Generate initial population
            LinkedList<T> successors = generateInitial();
            Tree<T> currentTree = data;

            // Calculate fitness
            while (true) {
                // This array will always be 16 in length as a 'hardcoded' population size
                // Also using an array to easily implement sorting
                Tuple<int, T>[] scoredSuccessors = new Tuple<int, T>[16];

                LinkedList<T>.Node currentNode = successors.First;

                for (int i = 0; i < 16; i++) {
                    scoredSuccessors[i] = new Tuple<int, T>(getScore(currentNode.data), currentNode.data);

                    currentNode = currentNode.next;
                }

                // Now that all population has a score assigned to it, sort the array
                scoredSuccessors = scoredSuccessors.OrderBy(t => t.Item1).ToArray();

                // Build the tree of best solutions of each generation (for printing at the end)
                currentTree.AddChild(scoredSuccessors[0].Item2);
                currentTree = currentTree.children.First.data;

                // If the best successor is a solution (score = 0), then return solution tree
                if (scoredSuccessors[0].Item1 == 0) return currentTree;

                // Otherwise "compute successors" using genetic operators
                LinkedList<T> bestSuccessors = new LinkedList<T>();

                Console.Write("Best scores: ");
                // First, choose best 4 successors
                for (int i=0; i < 4; i++) {
                    bestSuccessors.Append(scoredSuccessors[i].Item2);
                    Console.Write(scoredSuccessors[i].Item1 + " ");
                }
                Console.WriteLine();

                // Then use those to make next successors
                successors = computeSuccessors(bestSuccessors);
            }
        }

        // Helper function to build the list of states from initial to final
        LinkedList<T> SolutionSteps(Tree<T> solution) {
            LinkedList<T> steps = new LinkedList<T>();

            // We build the solution steps starting from the solution, using the parent to move backwards
            // We prepend each state of the board so at the end, the linked list will be in order from initial state to solution
            steps.Prepend(solution.value);
            while (solution.parent != null) {
                solution = solution.parent;
                steps.Prepend(solution.value);
            }

            return steps;
        }
    }

    // Generic Linked List class for holding the successors
    public class LinkedList<T> {
        public Node First { get; private set; }
        public Node Last { get; private set; }
        public int Length { get; private set; }

        public LinkedList() {
            First = null;
            Last = null;
            Length = 0;
        }

        // Append successors, set up node organization as well
        public void Append(T data) {
            Node node = new Node(data);
            // If first is null, then the list is empty, no need to check last
            if (First == null) {
                // Thus the new single node is both first and last
                First = node;
                Last = node;
            } else {
                // Otherwise, set up the pointers as you would normally
                Last.next = node;
                node.parent = Last;
                Last = node;
            }
            Length++;
        }

        public void Append(LinkedList<T> list) {
            Node node = list.First;
            while (node != null) {
                Append(node.data);
                node = node.next;
            }
        }

        public void Prepend(T data) {
            Node node = new Node(data);
            // If first is null then the list is empty
            if (First == null) {
                First = node;
                Last = node;
            } else {
                // Otherwise we add this new node to the front of the list
                node.next = First;
                First.parent = node;
                First = node;
            }
            Length++;
        }

        public void RemoveNode(Node node) {
            // Fix first/last pointers
            if (node == First) First = node.next;
            if (node == Last) Last = node.parent;

            // Ensure that no operations are ran on null
            if (node.parent != null) node.parent.next = node.next;
            if (node.next != null) node.next.parent = node.parent;

            Length--;
        }

        public void RemoveFirst() {
            RemoveNode(First);
        }

        public void RemoveLast() {
            RemoveNode(Last);
        }

        public Node GetNode(int index) {
            if (index < 0 || index >= Length) return null;

            Node node = First;
            for (int i = 0; i < index; i++) {
                node = node.next;
            }
            return node;
        }

        // Function to check if the data is already contained, to help avoid duplicate states
        public bool Contains(T data) {
            Node node = First;
            while (node != null) {
                if (data.Equals(node.data)) {
                    return true;
                } else {
                    node = node.next;
                }
            }
            return false;
        }

        // Returns true if the list is empty
        public bool isEmpty() {
            if (First == null) {
                return true;
            }
            return false;
        }

        // Generic node for organizing the list
        public class Node {
            public T data;
            public Node parent;
            public Node next;

            public Node(T newData) {
                data = newData;
                parent = null;
                next = null;
            }
        }
    }

    public class Tree<T> {
        public T value;
        public int Depth { private set; get; }
        public Tree<T> parent;
        public LinkedList<Tree<T>> children;

        public Tree(T data, Tree<T> parent = null) {
            value = data;
            children = new LinkedList<Tree<T>>();
            this.parent = parent;

            if (parent == null) {
                Depth = 0;
            } else {
                Depth = parent.Depth + 1;
            }
        }

        public Tree<T> AddChild(T data) {
            Tree<T> child = new Tree<T>(data, this);
            children.Append(child);
            return child;
        }

        public void AddChildren(LinkedList<T> list) {
            LinkedList<T>.Node node = list.First;

            while (node != null) {
                AddChild(node.data);
                node = node.next;
            }
        }
    }

    public class PuzzleState {
        protected int[] data;
        public int Width { private set; get; }

        public PuzzleState(int[] data, int width) {
            this.data = data;
            Width = width;
        }

        public PuzzleState(PuzzleState copy) {
            Width = copy.Width;
            data = new int[Width * Width];
            Array.Copy(copy.data, data, Width * Width);
        }

        // Indexer to allow us to use square brackets on this type
        public int this[int x, int y] {
            get { return data[x + y * Width]; }
            set { data[x + y * Width] = value; }
        }

        // Swap values of two locations (used for the movement functions)
        public void Swap(int x1, int y1, int x2, int y2) {
            int temp = this[x1, y1];
            this[x1, y1] = this[x2, y2];
            this[x2, y2] = temp;
        }

        // Override Equals function to use our own when compared with another of this object
        public override bool Equals(object obj) {
            if (obj is PuzzleState) return Equals((PuzzleState)obj);
            return base.Equals(obj);
        }

        // Using SequenceEqual to compare arrays, basically makes sure all values in array are in same order
        public bool Equals(PuzzleState state) {
            return data.SequenceEqual(state.data);
        }

        public void Print() {
            string output = string.Join(" ", data.Select(x => x.ToString()).ToArray());
            Console.WriteLine(output);
        }
    }

}
