# A* Pathfinding
This contains the project in which I implemented the A\* algorithm using the open-source map dataset from openstreetmap.org. This was implemented in the C# programming language.

This project was created using Visual Studio Community 2017.

Source code files are located in the folder: Pathfinding

The input .osm map files used in generation are not included.

**Program Overview**

This program is given an input osm file, which contains data on the nodes (points on the map) and ways (collection of points, represent pathways) and loads it into my own data structure I created to more easily navigate it. Once the loading is complete, it will prompt the user for start and end point coordinates (giving no input will use default coordinates of bottom-right to top-left).

Once the input is received, a timer will start and A\* will search for the shortest path. Once complete, the timer stops and the program will output the nodes travelled as well as the time taken for A\* to find the path in milliseconds. It will also prompt the user asking if they want a visual representation.

If the user says yes (inputs a y) then a Winform is created and the different routes are drawn using standard paint tools. Lastly, a thinner red line is drawn to represent the path found by A\*.

It should also be noted that this program does contain some legacy code for the initial test to make sure A\* was implemented correctly on a simpler dataset, before using the map dataset. The main logic for the legacy code is contained in the "Demonstration" function. The "MapData" function contains the main logic for the code used for the osm files.

**Visual Representation Example**

![Map Visual Representation](https://github.com/GloaNeko/Archive-School/blob/main/2021/AStar%20Pathfinding/VisualExample.png)
