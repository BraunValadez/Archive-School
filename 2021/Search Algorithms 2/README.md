# Search Algorithms 2
This contains the project in which I implemented 2 different search algorithms to solve the n-queens problem. It is worth noting that these algorithms were implemented in their most basic form, without optimizations, so it may take a long time for some solutions to be found, if it can be found at all. Additionally, one of the requirements was to use our own data structure, so I had to create my own tree data structure to store the data in. Lastly, this project inherited code from Search Algorithms 1, so some of the functions and operations may appear odd in implementation. If I were to implement these algorithms again, I would do so on their own rather than trying to force them into a single program.

This project was created using Visual Studio Community 2017.

Source code files are located in the folder: Assignment2

Output files are located in the folder: output

Note that the output files are labeled by their name with which problem they solved and using which algorithm. The number in the filename represents n. In the n-queens problem, a 0 represents an empty space and a 1 represents a queen. Also note that in the genetic algorithm, since it does not take a simple path, the in-between puzzle states are the best of each generation.

**Program Overview**

The program will ask the user for input on which problem to solve, what value to use for n, which algorithm to use, and lastly if they wish to have a file output of all the steps taken.

This program implements the following search algorithms:
* Hill Climbing
* Genetic Algorithm

The program will use those algorithms to find a solution for the following problem:
* n-queens problem
	-You have an n x n sized chees board, with n queens placed all in the first row. Your goal is to place the queens such that no queen is attacking another on the board.

All source code is contained in a single file: Program.cs
