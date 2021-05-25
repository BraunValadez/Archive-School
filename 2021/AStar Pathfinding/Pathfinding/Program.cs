using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Pathfinding {
    class Program {
        static void Main(string[] args) {
            Debug.Enabled = true;
            Application.EnableVisualStyles();

            Console.WriteLine("This program will run an A* search algorithm on a pre-determined map to test that the algorithm works correctly.");
            Console.WriteLine("When finished, the program will provide a few statistics such as how many moves the solution takes and the time it took to find that solution.\n");

            Console.WriteLine("Would you like a demonstration done on a simple 2D map, or to use map data from a file?");
            Console.WriteLine("1 - 2D Demonstration");
            Console.WriteLine("2 - Map data");
            Console.WriteLine("Please only input the integer of your desired choice.");

            int response;

            while (!int.TryParse(Console.ReadLine(), out response) || response < 1 || response > 2) {
                Console.WriteLine("Please input a valid integer of the option you would like to select.");
            }

            if (response == 2) {
                MapData();
            } else {
                Demonstration();
            }
        }

        public static void Demonstration() {
            // Generate the map
            int[] map = new int[] {
                1, 0, 2, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 2, 0, 0, 2, 0, 0, 3, 0,
                0, 0, 2, 0, 0, 2, 0, 0, 0, 0,
                0, 2, 2, 0, 0, 2, 2, 2, 2, 0,
                0, 0, 2, 0, 0, 0, 0, 0, 0, 0,
                2, 0, 2, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 2, 0, 0, 2, 2, 2, 0, 0,
                0, 2, 2, 0, 0, 0, 0, 2, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 2, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0
            };
            State initial = new State(State.ConvertMap(map), 10, 10);

            // Generate the necessary instances
            AStar<State> search = new AStar<State>(State.ScoreNode, State.CheckGoal, State.FindAdjacent);

            Console.WriteLine("A* is searching for the best path...");

            // Create stopwatch for keeping track of time until solution is found
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<State> solution = search.FindPath(initial); // Actually add initial state here
            watch.Stop();

            if (solution == null) {
                Console.WriteLine("A solution was not found for the problem.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                Environment.Exit(0);
            }

            Console.WriteLine("A solution was found!");
            Console.WriteLine("This took {0} ms and took {1} moves.", watch.ElapsedMilliseconds, solution.Count);

            Console.WriteLine("Would you like a text file of the steps? (y/n)");
            char response;

            while (!char.TryParse(Console.ReadLine(), out response) || char.ToLower(response) != 'y' && char.ToLower(response) != 'n') {
                Console.WriteLine("Please input a valid option, only a single char y or n.");
            }

            if (char.ToLower(response) == 'y') {
                string filename = "astar_solution.txt";

                try {
                    StreamWriter sw = new StreamWriter(filename);

                    foreach (State step in solution) {
                        for (int y = 0; y < step.Length; y++) {
                            for (int x = 0; x < step.Width; x++) {
                                sw.Write("{0} ", (int)step[x, y]);
                            }
                            sw.WriteLine();
                        }
                        sw.WriteLine();
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

        public static void MapData() {
            // Set up the filepath here
            string filepath = "map2.osm";

            Console.WriteLine("The pre-determined map comes from the file: {0}, which was exported from OpenStreetMap.org.", filepath);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            MapData map = new MapData(filepath);

            // Apply "filters" to the ways that can be used for navigation
            map.AddInclude("highway", "*");
            map.AddExclude("highway", "footway");
            map.AddExclude("highway", "cycleway");

            AStar<Node> search = new AStar<Node>(map.ScoreNode, map.CheckDestination, map.FindAdjacent);

            // Get input for the coordinates
            Console.WriteLine("Please input the starting point coordinates:");
            Console.WriteLine("Latitude first (positive is N, negative is S):");
            Console.WriteLine("*Note: Max latitude is {0}, Minimum latitude is {1}", map.MaxLat, map.MinLat);

            double initialLat;
            if (!double.TryParse(Console.ReadLine(), out initialLat)) {
                Console.WriteLine("Error in input, using default value instead. (MinLat)");
                initialLat = map.MinLat;
            }

            Console.WriteLine("Longitude (positive is E, negative is W):");
            Console.WriteLine("*Note: Max longitude is {0}, Minimum longitude is {1}", map.MaxLon, map.MinLon);

            double initialLon;
            if (!double.TryParse(Console.ReadLine(), out initialLon)) {
                Console.WriteLine("Error in input, using default value instead. (MaxLon)");
                initialLon = map.MaxLon;
            }

            Console.WriteLine("Now, input the destination coordinates:");
            Console.WriteLine("Latitude first (positive is N, negative is S):");
            Console.WriteLine("*Note: Max latitude is {0}, Minimum latitude is {1}", map.MaxLat, map.MinLat);

            double destLat;
            if (!double.TryParse(Console.ReadLine(), out destLat)) {
                Console.WriteLine("Error in input, using default value instead. (MaxLat)");
                destLat = map.MaxLat;
            }

            Console.WriteLine("Longitude (positive is E, negative is W):");
            Console.WriteLine("*Note: Max longitude is {0}, Minimum longitude is {1}", map.MaxLon, map.MinLon);

            double destLon;
            if (!double.TryParse(Console.ReadLine(), out destLon)) {
                Console.WriteLine("Error in input, using default value instead. (MinLon)");
                destLon = map.MinLon;
            }

            // Mark destination and start nodes
            map.SetDestination(map.GetNode(destLat, destLon));

            Node initial = map.GetNode(initialLat, initialLon);

            Console.WriteLine("A* is searching for the best path...");

            // Create stopwatch for keeping track of time until solution is found
            Stopwatch watch = new Stopwatch();
            watch.Start();
            List<Node> solution = search.FindPath(initial);
            watch.Stop();

            if (solution == null) {
                Console.WriteLine("A solution was not found for the problem.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                Environment.Exit(0);
            }

            Console.WriteLine("A solution was found!");
            Console.WriteLine("This took {0} ms and took {1} moves.", watch.ElapsedMilliseconds, solution.Count);

            Console.WriteLine("Would you like a visual representation of the path? (y/n)");
            char response;

            while (!char.TryParse(Console.ReadLine(), out response) || char.ToLower(response) != 'y' && char.ToLower(response) != 'n') {
                Console.WriteLine("Please input a valid option, only a single char y or n.");
            }

            if (response == 'y') {
                // Use WinForms for visual representation
                MapForm form = new MapForm(map, solution);

                form.ShowDialog();
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    class MapForm : Form {
        const int WIDTH = 1500;
        double mapRatio;
        double latRatio;
        double lonRatio;
        MapData map;
        List<Node> solution;

        PictureBox pb;
        Button btn;

        Pen mapLine;
        Pen pathLine;
        Pen pathEnd;

        public MapForm(MapData map, List<Node> solution) : base() {
            this.map = map;
            this.solution = solution;
            mapRatio = Math.Abs(map.MaxLat - map.MinLat) / Math.Abs(map.MaxLon - map.MinLon);
            lonRatio = WIDTH / (map.MaxLon - map.MinLon);
            latRatio = (int)(WIDTH * mapRatio) / (map.MaxLat - map.MinLat);
            Load += new EventHandler(form_Load);

            mapLine = new Pen(Color.DarkGray, 10) {
                EndCap = System.Drawing.Drawing2D.LineCap.Round,
                StartCap = System.Drawing.Drawing2D.LineCap.Round
            };
            pathLine = new Pen(Color.Red, 4) {
                EndCap = System.Drawing.Drawing2D.LineCap.Round,
                StartCap = System.Drawing.Drawing2D.LineCap.Round
            };
            pathEnd = new Pen(Color.Red, 4) {
                EndCap = System.Drawing.Drawing2D.LineCap.Custom,
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 3)
            };
        }

        private void form_Load(object sender, EventArgs e) {
            Width = WIDTH + 10;
            Height = (int)(85 + (WIDTH * mapRatio));
            Text = "Found Path";
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            // Create a PictureBox as a container for all the map objects
            pb = new PictureBox() {
                Width = WIDTH,
                Height = (int)(WIDTH * mapRatio),
                Left = 5,
                Top = 5,
                BackColor = Color.White
            };
            pb.Paint += new PaintEventHandler(pb_Paint);
            Controls.Add(pb);

            // Create a simple Close/OK button
            btn = new Button() {
                Width = 120,
                Height = 30,
                Left = this.Width / 2 - 60,
                Top = this.Height - 75,
                Text = "Close",
                DialogResult = DialogResult.OK
            };
            Controls.Add(btn);
        }

        private void pb_Paint(object sender, PaintEventArgs e) {
            float x1, y1, x2, y2;

            foreach (Way way in map.GetWays()) {
                // Skip any ways not used in the pathfinding
                if (!map.IsValidWay(way)) {
                    continue;
                }

                List<Node> nodes = way.GetNodes();

                for (int i = 1; i < nodes.Count; i++) {
                    if (map.Contains(nodes[i - 1]) || map.Contains(nodes[i])) {
                        // Calculate the x/y coordinates for each node
                        x1 = (float)((nodes[i - 1].Longitude - map.MinLon) * lonRatio);
                        y1 = (float)((nodes[i - 1].Latitude - map.MinLat) * latRatio);
                        x2 = (float)((nodes[i].Longitude - map.MinLon) * lonRatio);
                        y2 = (float)((nodes[i].Latitude - map.MinLat) * latRatio);

                        // Draw the lines for each road
                        e.Graphics.DrawLine(mapLine, x1, pb.Height - y1, x2, pb.Height - y2);
                    }
                }
            }

            // Draw the solution path (except the end)
            for (int i = 1; i < solution.Count - 1; i++) {
                if (map.Contains(solution[i - 1]) || map.Contains(solution[i])) {
                    // Calculate the x/y coordinates for each node
                    x1 = (float)((solution[i - 1].Longitude - map.MinLon) * lonRatio);
                    y1 = (float)((solution[i - 1].Latitude - map.MinLat) * latRatio);
                    x2 = (float)((solution[i].Longitude - map.MinLon) * lonRatio);
                    y2 = (float)((solution[i].Latitude - map.MinLat) * latRatio);

                    // Draw the lines for the solution
                    e.Graphics.DrawLine(pathLine, x1, pb.Height - y1, x2, pb.Height - y2);
                }
            }

            // Draw the solution end
            x1 = (float)((solution[solution.Count - 2].Longitude - map.MinLon) * lonRatio);
            y1 = (float)((solution[solution.Count - 2].Latitude - map.MinLat) * latRatio);
            x2 = (float)((solution[solution.Count - 1].Longitude - map.MinLon) * lonRatio);
            y2 = (float)((solution[solution.Count - 1].Latitude - map.MinLat) * latRatio);

            e.Graphics.DrawLine(pathEnd, x1, pb.Height - y1, x2, pb.Height - y2);

            // Draw a bigger circle on the start point to make it more visible
            x1 = (float)((solution[0].Longitude - map.MinLon) * lonRatio);
            y1 = (float)((solution[0].Latitude - map.MinLat) * latRatio);

            e.Graphics.FillEllipse(Brushes.Red, x1 - 6, pb.Height - y1 - 6, 12, 12);
        }
    }

    class AStar<T> {
        List<Tuple<Tree<T>, float>> openNodes, closedNodes;
        Func<Tree<T>, float> scoreNode;
        Func<T, bool> checkGoal;
        Func<Tree<T>, List<Tree<T>>> findAdjacent;

        public AStar(Func<Tree<T>, float> scoreNode, Func<T, bool> checkGoal, Func<Tree<T>, List<Tree<T>>> findAdjacent) {
            this.scoreNode = scoreNode;
            this.checkGoal = checkGoal;
            this.findAdjacent = findAdjacent;
        }

        public List<T> FindPath(T initial) {
            // Declare new lists for explored nodes (closed) and nodes to explore (open)
            openNodes = new List<Tuple<Tree<T>, float>>();
            closedNodes = new List<Tuple<Tree<T>, float>>();

            // Solve the problem
            Tree<T> solution = Solve(initial);

            if (solution == null) {
                return null;
            }

            return SolutionSteps(solution);
        }

        Tree<T> Solve(T initial) {
            // Make the initial tree and add it to the open node list
            Tree<T> initialTree = new Tree<T>(initial);

            openNodes.Add(new Tuple<Tree<T>, float>(initialTree, scoreNode(initialTree)));

            while(openNodes.Count > 0) {
                // Sort it such that the lowest score (the best) is first in the list
                openNodes.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                // Mark the current node
                Tuple<Tree<T>, float> current = openNodes[0];

                // Remove it from the open nodes since it's being processed
                openNodes.RemoveAt(0);

                // Find all adjacent spaces (aka compute successors)
                List<Tree<T>> adjacent = findAdjacent(current.Item1);

                // For each of the adjacent nodes...
                foreach(Tree<T> node in adjacent) {
                    // Check if we have found the goal
                    if (checkGoal(node.value)) return node;

                    // Otherwise convert into tuple (calculate its score)
                    Tuple<Tree<T>, float> tupleNode = new Tuple<Tree<T>, float>(node, scoreNode(node));

                    // Then only add it to the open list if it does not exist or is better than any similar node
                    if (!NodeExists(tupleNode)) openNodes.Add(tupleNode);
                }

                // Once all successors are processed, add current node to closedNodes
                closedNodes.Add(current);
            }

            // If we exit the while loop then no solution was found
            return null;
        }

        bool NodeExists(Tuple<Tree<T>, float> node) {
            foreach(Tuple<Tree<T>, float> openNode in openNodes.Where(t => t.Item1.value.Equals(node.Item1.value))) {
                if (openNode.Item2 <= node.Item2) {
                    return true;
                }
            }
            foreach (Tuple<Tree<T>, float> closedNode in closedNodes.Where(t => t.Item1.value.Equals(node.Item1.value))) {
                if (closedNode.Item2 <= node.Item2) {
                    return true;
                }
            }
            return false;
        }

        // Helper function to build the list of states from initial to final
        List<T> SolutionSteps(Tree<T> solution) {
            List<T> steps = new List<T>();

            // We build the solution steps starting from the solution, using the parent to move backwards
            // We prepend each state of the board so at the end, the linked list will be in order from initial state to solution
            steps.Add(solution.value);
            while (solution.parent != null) {
                solution = solution.parent;
                steps.Add(solution.value);
            }
            
            steps.Reverse();
            return steps;
        }
    }

    public class Tree<T> {
        public T value;
        public Tree<T> parent;
        public List<Tree<T>> children;

        public Tree(T data, Tree<T> parent = null) {
            value = data;
            children = new List<Tree<T>>();
            this.parent = parent;
        }

        public Tree<T> AddChild(T data) {
            Tree<T> child = new Tree<T>(data, this);
            children.Add(child);
            return child;
        }

        public void AddChildren(List<T> list) {
            foreach(T data in list) {
                AddChild(data);
            }
        }
    }

    public class State {
        public enum Tile: int {
            Empty = 0,
            Agent = 1,
            Wall = 2,
            Goal = 3
        }

        protected Tile[] data;
        public int Width { private set; get; }
        public int Length { private set; get; }
        protected int AgentX = -1;
        protected int AgentY = -1;
        protected int GoalX = -1;
        protected int GoalY = -1;

        public State(Tile[] data, int width, int length) {
            this.data = data;
            Width = width;
            Length = length;

            for (int y = 0; y < Length; y++) {
                for (int x = 0; x < Width; x++) {
                    if (this[x, y] == Tile.Agent) {
                        AgentX = x;
                        AgentY = y;
                    }
                    if (this[x, y] == Tile.Goal) {
                        GoalX = x;
                        GoalY = y;
                    }
                }
            }
        }

        public State(State copy) {
            Width = copy.Width;
            Length = copy.Length;
            AgentX = copy.AgentX;
            AgentY = copy.AgentY;
            GoalX = copy.GoalX;
            GoalY = copy.GoalY;
            data = new Tile[Length * Width];
            Array.Copy(copy.data, data, Length * Width);
        }

        public State(int length, int width) {
            Length = length;
            Width = width;
            data = new Tile[Length * Width];
        }

        public static float ScoreNode(Tree<State> node) {
            float distToGoal = (float)Math.Sqrt(Math.Pow(node.value.AgentX - node.value.GoalX, 2) + Math.Pow(node.value.AgentY - node.value.GoalY, 2));
            float distTravelled = 0;

            Tree<State> parent = node.parent;
            Tree<State> child = node;
            while (parent != null) {
                distTravelled++;
                if (child.value.AgentX - parent.value.AgentX != 0 && child.value.AgentY - parent.value.AgentY != 0) {
                    distTravelled += 0.4f;
                }
                child = parent;
                parent = parent.parent;
            }

            return distToGoal + distTravelled;
        }

        public static bool CheckGoal(State node) {
            if(node.GoalX == node.AgentX && node.GoalY == node.AgentY) {
                return true;
            }
            return false;
        }

        public static List<Tree<State>> FindAdjacent(Tree<State> node) {
            List<Tree<State>> adjacent = new List<Tree<State>>();

            for (int i = -1; i < 2; i++) {
                for (int j = -1; j < 2; j++) {
                    // Skip if the change in x/y is 0 (because that's the same space)
                    if (i == 0 && j == 0) {
                        continue;
                    }
                    State move = node.value.Move(i, j);
                    // If move is not null ("moved" the agent sucessfully), then add to the return list
                    if (move != null) {
                        adjacent.Add(node.AddChild(move));
                    }
                }
            }

            return adjacent;
        }

        public static Tile[] ConvertMap(int[] map) {
            Tile[] tileMap = new Tile[map.Length];

            for(int i = 0; i < tileMap.Length; i++) {
                tileMap[i] = (Tile)map[i];
            }

            return tileMap;
        }

        // Indexer to allow us to use square brackets on this type
        public Tile this[int x, int y] {
            get { return data[x + y * Width]; }
            set {
                if (value == Tile.Agent) {
                    if (AgentX >= 0) this[AgentX, AgentY] = Tile.Empty;
                    AgentX = x;
                    AgentY = y;
                }
                if (value == Tile.Goal) {
                    if (GoalX >= 0) this[GoalX, GoalY] = Tile.Empty;
                    GoalX = x;
                    GoalY = y;
                }
                data[x + y * Width] = value;
            }
        }

        // Move the agent using this function
        public State Move(int dx, int dy) {
            int newX = AgentX + dx;
            int newY = AgentY + dy;
            
            // Check that we are within the bounds of the array, and that the new tile is accessible (not a wall)
            if (newX >= 0 && newX < Width && newY >= 0 && newY < Length && this[newX, newY] != Tile.Wall) {
                State ret = new State(this);
                ret[newX, newY] = Tile.Agent;
                return ret;
            }

            return null;
        }

        // Override Equals function to use our own when compared with another of this object
        public override bool Equals(object obj) {
            if (obj is State) return Equals((State)obj);
            return base.Equals(obj);
        }

        // Using SequenceEqual to compare arrays, basically makes sure all values in array are in same order
        public bool Equals(State state) {
            return data.SequenceEqual(state.data);
        }


        public void Print() {
            string output = string.Join(" ", data.Select(x => x.ToString()).ToArray());
            Console.WriteLine(output);
        }
    }

    public class MapData {
        public double MinLat { get; private set; }
        public double MinLon { get; private set; }
        public double MaxLat { get; private set; }
        public double MaxLon { get; private set; }
        List<Node> nodes;
        List<Way> ways;
        List<Tuple<string, string>> include;
        List<Tuple<string, string>> exclude;
        Node destination;

        // Constructor reads from a file and converts the XML file to MapData, Ways, and Nodes
        public MapData(string filepath) {
            nodes = new List<Node>();
            ways = new List<Way>();
            include = new List<Tuple<string, string>>();
            exclude = new List<Tuple<string, string>>();

            Debug.Write("Loading map data from " + filepath);

            // Read from file into an XML Document
            StreamReader sr = new StreamReader(filepath);

            XDocument map = XDocument.Parse(sr.ReadToEnd());

            // Convert the XML document to our datasets of nodes, ways, mapdata
            foreach(XElement element in map.Element("osm").Descendants()) {
                if (element.Name == "node") {
                    // If the element we're parsing is a node, create a new node
                    Node node = new Node(ulong.Parse(element.Attribute("id").Value), double.Parse(element.Attribute("lat").Value), double.Parse(element.Attribute("lon").Value));

                    Debug.Write("  Creating node with id = " + node.ID + ", lat = " + node.Latitude + ", long = " + node.Longitude);
                    
                    // If the element has child elements, we know those are tags so add them as such to the node
                    if (element.HasElements) {
                        foreach(XElement tag in element.Descendants()) {
                            string key = tag.Attribute("k").Value;
                            string val = tag.Attribute("v").Value;

                            node.AddTag(key, val);
                            Debug.Write("    Adding tag: " + key + " = " + val);
                        }
                    }
                    // Actually add the node to the list
                    nodes.Add(node);
                // If the element we're parsing is a way, create a new way
                } else if (element.Name == "way") {
                    Way way = new Way();

                    Debug.Write("  Creating Way...");

                    // Since it's organized by child elements, go through each child element
                    foreach(XElement e in element.Descendants()) {
                        // If the name of the element is "nd" this is a node reference, find the node and add it to the way
                        if (e.Name == "nd") {
                            ulong id = ulong.Parse(e.Attribute("ref").Value);
                            Node node = nodes.Where(o => o.ID == id).First();

                            Debug.Write("    Adding node " + id + " to way.");

                            node.AddWay(way);
                            way.AddNode(node);
                        // Otherwise if it's a tag, add it as such
                        } else if (e.Name == "tag") {
                            string key = e.Attribute("k").Value;
                            string val = e.Attribute("v").Value;

                            way.AddTag(key, val);
                            Debug.Write("    Adding tag: " + key + " = " + val);
                        }
                    }
                    // Actually add the way to the list
                    ways.Add(way);
                // The bounds only exist once at the top of the file, so they are tested for last to save skipped if statement checks
                } else if (element.Name == "bounds") {
                    // Simply save all values as named
                    MinLat = double.Parse(element.Attribute("minlat").Value);
                    MinLon = double.Parse(element.Attribute("minlon").Value);
                    MaxLat = double.Parse(element.Attribute("maxlat").Value);
                    MaxLon = double.Parse(element.Attribute("maxlon").Value);

                    Debug.Write("  Setting bounds: " + MinLat + ", " + MinLon + ", " + MaxLat + ", " + MaxLon);
                }
            }

            Debug.Write("Loading complete.");

            Debug.Write("Total nodes: " + nodes.Count);
            Debug.Write("Total ways: " + ways.Count);
        }

        public Node GetNode(double lat, double lon) {
            Node match = null;
            double matchDistance = double.MaxValue;

            foreach (Way way in ways) {
                // If the way is not valid, skip it
                if (!IsValidWay(way)) {
                    continue;
                }

                // If we made it here, then this way meets the criteria
                // Now we find the closest node to the given lat/long
                foreach(Node node in way.GetNodes()) {
                    // If the node is not within the specified coordinates, then skip it
                    if (!Contains(node)) {
                        continue;
                    }

                    // Otherwise calculate the distance, and save the best
                    double distance = node.GetDistance(lat, lon);

                    if (distance < matchDistance) {
                        matchDistance = distance;
                        match = node;
                    }
                }
            }

            return match;
        }

        public List<Way> GetWays() {
            return ways;
        }

        public void SetDestination(Node node) {
            destination = node;
        }

        public void AddInclude(string key, string value) {
            include.Add(new Tuple<string, string>(key, value));
        }

        public void AddExclude(string key, string value) {
            exclude.Add(new Tuple<string, string>(key, value));
        }

        public bool Contains(Node node) {
            return node.Latitude >= MinLat && node.Latitude <= MaxLat && node.Longitude >= MinLon && node.Longitude <= MaxLon;
        }

        public bool IsValidWay(Way way) {
            bool useWay = false;

            // Check if current way contains at least one tag in include
            foreach (Tuple<string, string> tag in include) {
                string val = way.Tag(tag.Item1);
                if (val != null && (tag.Item2 == "*" || val == tag.Item2)) {
                    useWay = true;
                    break;
                }
            }

            // Check if there is an excluded tag in the current way
            foreach (Tuple<string, string> tag in exclude) {
                if (way.Tag(tag.Item1) == tag.Item2) {
                    useWay = false;
                    break;
                }
            }

            return useWay;
        }

        public float ScoreNode(Tree<Node> tree) {
            double distToGoal = tree.value.GetDistance(destination);
            double distTravelled = 0;

            Tree<Node> parent = tree.parent;
            Tree<Node> child = tree;
            while (parent != null) {
                distTravelled += child.value.GetDistance(parent.value);
                child = parent;
                parent = child.parent;
            }

            return (float)(distToGoal + distTravelled);
        }

        public bool CheckDestination(Node node) {
            if (node == destination) {
                return true;
            }

            return false;
        }

        public List<Tree<Node>> FindAdjacent(Tree<Node> tree) {
            List<Tree<Node>> ret = new List<Tree<Node>>();

            foreach (Way way in tree.value.GetWays()) {
                int index = way.GetNodes().IndexOf(tree.value);
                if (index > 0) {
                    ret.Add(tree.AddChild(way.GetNodes()[index - 1]));
                }
                if (index < way.GetNodes().Count - 1) {
                    ret.Add(tree.AddChild(way.GetNodes()[index + 1]));
                }
            }

            return ret;
        }
    }

    public class Node {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public ulong ID { get; private set; }
        Dictionary<string, string> tags;
        List<Way> ways;

        public Node(ulong id, double latitude, double longitude) {
            ID = id;
            Latitude = latitude;
            Longitude = longitude;

            tags = new Dictionary<string, string>();
            ways = new List<Way>();
        }

        public void AddTag(string key, string value) {
            tags.Add(key, value);
        }

        public void AddWay(Way way) {
            ways.Add(way);
        }

        public string Tag(string key) {
            if (tags.ContainsKey(key)) {
                return tags[key];
            }

            return null;
        }

        public List<Way> GetWays() {
            return ways;
        }

        public double GetDistance(double lat, double lon) {
            return Math.Sqrt(Math.Pow(Math.Abs(Latitude - lat), 2) + Math.Pow(Math.Abs(Longitude - lon), 2));
        }

        public double GetDistance(Node node) {
            return GetDistance(node.Latitude, node.Longitude);
        }
    }

    public class Way {
        List<Node> nodes;
        Dictionary<string, string> tags;

        public Way() {
            nodes = new List<Node>();
            tags = new Dictionary<string, string>();
        }

        public void AddNode(Node node) {
            nodes.Add(node);
        }

        public void AddTag(string key, string value) {
            tags.Add(key, value);
        }

        public string Tag(string key) {
            if (tags.ContainsKey(key)) {
                return tags[key];
            }

            return null;
        }

        public List<Node> GetNodes() {
            return nodes;
        }
    }

    public static class Debug {
        public static bool Enabled = false;

        public static void Write(string str) {
            if (Enabled) {
                Console.WriteLine(str);
            }
        }
    }
}
