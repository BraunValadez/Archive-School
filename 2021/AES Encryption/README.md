# AES Encryption
This contains the project in which I implemented the AES Encryption algorithm in C++, along with certain output files. This readme will cover what each file is, as well as a high level overview of the program.

This project was created using Visual Studio Community 2017.

All files are located in the folder: CS4920_HW3_PP3

**Output Files:**
* BitComparison.txt
	-Part of this assignment was to show how AES exhibits the "Avalanche Effect." In this case, we changed the plaintext by 1 bit and then compared the outputs between each AES round to see how many bits were different between the two. This file provides both plaintexts, the key used, and the text after each round as well as how many bits are different. This is all provided in a table format.
* KeyExpansion.txt
	-This file contains the different steps during the Key Expansion algorithm and stores each word of the key. Additionally, this file shows the results of the functions used to reach them. It also contains the used key at the top of the file. Once again, this is provided in a table format.
* Transformations.txt
	-This file contains the matrix representation of the plaintext and round key. This also shows the transformations done on the plaintext after each function of AES. As with the other files, it contains the plaintext and key used at the top of the file and presents the data in a table format.

**Program Overview**

This program is given a key and plaintext (provided in the source code) and will first expand the key using the Key Expansion algorithm to generate each round key. Once completed, the program will run the AES algorithm on the plaintext while building the output files as it is ran.

It should be noted that including keys in the program is insecure, and this was only done for ease of re/usability as this originated as a school assignment, not a proper, secure implementation of AES.

The source code is contained in the same folder mentioned above. Filename: CS4920_HW3_PP3.cpp
