# Search Algorithms 1
This contains the project in which I implemented 4 different search algorithms to solve two different problems. It is worth noting that these algorithms were implemented in their most basic form, without optimizations, so it may take a long time for some solutions to be found. Additionally, one of the requirements was to use our own data structure, so I had to create my own tree data structure to store the data in.

This project was created using Visual Studio Community 2017.

Source code files are located in the folder: Assignment1

Output files are located in the folder: output

Note that the output files are labeled by their name with which problem they solved and using which algorithm. The number in the filename represents n. In the n-puzzle problem, the tiles are numbered 1 through n, where 0 is the empty space. In the n-queens problem, a 0 represents an empty space and a 1 represents a queen.

**Program Overview**

The program will ask the user for input on which problem to solve, what value to use for n, which algorithm to use, and lastly if they wish to have a file output of all the steps taken. There is an extra input step for the n-puzzle problem, which is how many times you wish to shuffle the board to create your starting point - similar to mixing up a Rubix cube before solving it.

This program implements the following search algorithms:
* Depth First
* Breadth First
* Iterative Deepening
* Bi-directional

The program will use those algorithms to find solutions for the following problems:
* n-puzzle problem
	-You have a square puzzle with n pieces (such that n+1 is a square number) and can only move one piece at a time. Your goal is to put the pieces in sequential order.
	-Note that when defining n in the user input, this program takes it very literally. Ensure that n+1 is a square number, and not the size of the board like in the n-queens problem!
* n-queens problem
	-You have an n x n sized chess board, with n queens placed all in the first row. Your goal is to place the queens such that no queen is attacking another on the board.
	
All source code is contained in a single file: Program.cs
