#define _CRT_SECURE_NO_WARNINGS
// The above line was to ignore warnings for using sprintf which is a deprecated function later in the program

#include <fstream>
#include <iomanip>
#include <iostream>
#include <string>

using namespace std;

// For more readability, we designate each unsigned char to use the name "byte" instead
// since this is not C++17 with the actual byte type
// Same with using uints for words
typedef unsigned char byte;
typedef unsigned int word;

void KeyExpansion(byte[], word[]);
word SubWord(word);
word RotWord(word);
void PrintKeyExpansion(word[], byte[]);
void SubBytes(byte[]);
void ShiftRows(byte[]);
void MixColumns(byte[]);
void AddRoundKey(byte[], word[], int);
void WriteInitialRound(byte[], byte[], FILE*);
void WriteRound(byte[], byte[], byte[], byte[], word[], int, FILE*);
void WritePostRounds(byte[], FILE*);
int CompareBits(byte[], byte[]);
void WriteInitialComparison(byte[], byte[], byte[], byte[], byte[], FILE*);
void WriteRoundComparison(byte[], byte[], int, FILE*);


// Defining a global s-box here with constants
// Due to complications with programming problem 1, I have to statically define the s-box here
// Rather than constructing it using multiplicative inverse
// Accessed using sBox[x][y]
byte sbox[16][16] = {
	{0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76},
	{0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0},
	{0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15},
	{0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75},
	{0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84},
	{0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf},
	{0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8},
	{0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2},
	{0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73},
	{0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb},
	{0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79},
	{0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08},
	{0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a},
	{0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e},
	{0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf},
	{0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16}
};

// RC bit values used to define Rcon as described in the textbook
byte RC[10] = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36 };

int main()
{
	// All keys used will be left in comments here
	// testKey is the key used in a textbook example which I used to check that everything was functioning properly
	byte key[16] = { 0x0f, 0x00, 0x71, 0xc9, 0x47, 0xd9, 0xe8, 0x59, 0x1c, 0xb7, 0xad, 0xd6, 0xaf, 0x7f, 0x67, 0x98 }; // Key for Problem 3b
	// byte testKey[16] = { 0x0f, 0x15, 0x71, 0xc9, 0x47, 0xd9, 0xe8, 0x59, 0x0c, 0xb7, 0xad, 0xd6, 0xaf, 0x7f, 0x67, 0x98 }; // Test key from textbook

	// Plaintext is provided here
	// testPlaintext is the plaintext given in the textbook example for checking answers
	byte plaintextBase[16] = { 0x02, 0x00, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10 }; // Plaintext for Problem 3b (baseline)
	byte plaintext[16] = { 0x02, 0x01, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10 }; // Plaintext for Problem 3b (edit 1)
	// byte plaintext[16] = { 0x03, 0x00, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10 }; // Plaintext for Problem 3b (edit 2)
	// byte plaintext[16] = { 0x02, 0x00, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x11 }; // Plaintext for Problem 3b (edit 3)
	// byte testPlaintext[16] = { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10}; // Test plaintext from textbook

	// State gets initialized with the plaintext so we can print the plaintext out later
	byte stateBase[16];
	byte state[16];
	for (int i = 0; i < 16; i++) {
		state[i] = plaintext[i];
		stateBase[i] = plaintextBase[i];
	}

	// Below is debug code for printing out the key
	/* for (int i = 0; i < 16; i++) {
		cout << hex << setfill('0') << setw(2) << (int)testKey[i] << " ";
	}
	cout << "\n"; */

	// Word array for storing round keys
	word wordArray[44];
	KeyExpansion(key, wordArray);

	// Below is debug code used for printing out the whole word array
	/* for (int i = 0; i < 44; i++) {
		if (i % 4 == 0 && i != 0) cout << "\n";
		cout << hex << setfill('0') << setw(8) << wordArray[i] << " ";
	}
	cout << "\n"; */

	// Perform key expansion
	PrintKeyExpansion(wordArray, key);
	cout << "Key expansion completed.\n";

	// AES starts here
	// First, we xor plaintext with the key
	for (int i = 0; i < 16; i++) {
		state[i] = state[i] ^ key[i];
		stateBase[i] = stateBase[i] ^ key[i];
	}

	// Open files for output
	FILE *file = NULL;
	FILE *comparisonFile = NULL;
	// Transformations is for problem 2, but is left here just in case required
	fopen_s(&file, "Transformations.txt", "w");
	fopen_s(&comparisonFile, "BitComparison.txt", "w");

	// Write the initial round
	WriteInitialRound(key, plaintext, file);

	WriteInitialComparison(plaintextBase, plaintext, stateBase, state, key, comparisonFile);

	// Now we begin the individual rounds
	// We also save "state" before each function to write it to a file at the end of each round
	// The logic for saving the state is still here from problem 2, the only difference is we make each round
	// function called twice, one for the "Base State" the other for the normal "State"
	for (int i = 0; i < 10; i++) {
		byte initialState[16];
		for (int j = 0; j < 16; j++) {
			initialState[j] = state[j];
		}

		byte postSub[16];
		SubBytes(state);
		SubBytes(stateBase);
		for (int j = 0; j < 16; j++) {
			postSub[j] = state[j];
		}

		byte postShift[16];
		ShiftRows(state);
		ShiftRows(stateBase);
		for (int j = 0; j < 16; j++) {
			postShift[j] = state[j];
		}

		byte postMix[16];
		// We only do this if it is not the last round (last round is when i = 9)
		if (i < 9) {
			MixColumns(state);
			MixColumns(stateBase);
			for (int j = 0; j < 16; j++) {
				postMix[j] = state[j];
			}
		}

		// Finally, add the round key
		AddRoundKey(state, wordArray, i);
		AddRoundKey(stateBase, wordArray, i);

		// Below is debug code for displaying each of the states for each round
		/* cout << "Initial: ";
		for (int i = 0; i < 16; i++) {
			cout << hex << setfill('0') << setw(2) << (int)initialState[i] << " ";
		}
		cout << "\n";

		cout << "postSub: ";
		for (int i = 0; i < 16; i++) {
			cout << hex << setfill('0') << setw(2) << (int)postSub[i] << " ";
		}
		cout << "\n";

		cout << "postShift: ";
		for (int i = 0; i < 16; i++) {
			cout << hex << setfill('0') << setw(2) << (int)postShift[i] << " ";
		}
		cout << "\n";

		cout << "postMix: ";
		for (int i = 0; i < 16; i++) {
			cout << hex << setfill('0') << setw(2) << (int)postMix[i] << " ";
		}
		cout << "\n\n"; */


		// Write to file at end of each round
		WriteRound(initialState, postSub, postShift, postMix, wordArray, i, file);
		
		// Now for problem 3, we write the comparison of the two texts as well
		WriteRoundComparison(stateBase, state, i, comparisonFile);
	}

	// After each round, we write the ciphertext to the file to adhere to the table format
	WritePostRounds(state, file);
	// Close the files
	fclose(file);
	fclose(comparisonFile);

	cout << "AES encryption completed.\n";
}

// Implemented as described by pseudocode in the textbook
void KeyExpansion(byte key[16], word w[44]) {
	word temp;

	// Copy the key into the first 4 indecies of the array
	for (int i = 0; i < 4; i++) {
		// This simply adds and shifts the different bytes into their position to form the word at index i
		w[i] = (key[4 * i] << 24) + (key[4 * i + 1] << 16) + (key[4 * i + 2] << 8) + (key[4 * i + 3]);
	}

	// Fill in the rest using the Key Expansion formula
	for (int i = 4; i < 44; i++) {
		temp = w[i - 1];
		word Rcon = RC[(i / 4) - 1] << 24;
		if (i % 4 == 0) {
			temp = SubWord(RotWord(temp)) ^ Rcon;
		}
		w[i] = w[i - 4] ^ temp;
	}
}

// Uses the S-box to substitute each byte of the word
word SubWord(word w) {
	// Create a pointer to each of the w's bytes
	byte *wptr = (byte*)&w;

	for (int i = 0; i < 4; i++) {
		// Store initial byte at index i to another byte
		byte b = wptr[i];

		// Remove initial byte at index i <-- can be moved later if necessary
		word remover = 0xffffff00;
		for (int j = 0; j < i; j++) {
			remover = remover << 8;
			remover += 0xff;
		}
		w = w & remover;

		// Split new byte into nibbles
		int leftBits = b >> 4;
		int rightBits = b & 0x0f;

		// Use nibbles to find indexes of sbox for replacement byte
		byte subByte = sbox[leftBits][rightBits];

		// Add new byte to the word, left shifted as necessary (8 * i)
		word newWord = 0;
		newWord += subByte;
		newWord = newWord << 8 * i;

		w = w + newWord;
	}

	// Once sub is finished we return the shifted word
	return w;
}

// Returns a cyclic left shift of the input word
word RotWord(word w) {
	// Create a pointer to each of w's bytes
	byte *wptr = (byte*)&w;
	// Store a backup of the first byte
	byte b = wptr[3];
	// Left shift by 1 byte (8 bits)
	w = w << 8;
	// Return the resulting word after moving the leftmost byte to the right side
	return w += b;
}

void PrintKeyExpansion(word w[], byte key[]) {
	// Open file for output
	FILE *file = NULL;
	fopen_s(&file, "KeyExpansion.txt", "w");

	fprintf(file, "Key: ");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", key[i]);
	}
	fprintf(file, "\n\n");

	fprintf(file, "%-32s| Auxiliary Function\n", "Key Words");

	// Print the key expansion functions and results for each of the 10 rounds
	for (int i = 0; i < 11; i++) {
		// Calculate all necessary values, given same names as in table
		word x = RotWord(w[4 * i + 3]);
		word y = SubWord(x);
		word Rcon = RC[i] << 24;
		word z = y ^ Rcon;

		fprintf(file, "---------------------------------------------------------------------\n");

		// Now print to file
		for (int j = 0; j < 4; j++) {
			// Using a cstring "buffer" to buffer the input to the left side for nice formatting
			char buffer[32];
			buffer[0] = '\0';

			// Make a word pointer to the start of the desired word
			byte *wptr = (byte*)w + (4 * i + j) * sizeof(word);

			// Print the left side of the table
			sprintf(buffer, "w%d = ", 4 * i + j);
			// Logic for handling adding the "xor" part of the left side, along with w vs z
			if (i != 0) {
				if (j == 0) {
					sprintf(buffer + strlen(buffer), "w%d ^ z%d = ", 4 * i - 4, i);
				}
				else {
					sprintf(buffer + strlen(buffer), "w%d ^ w%d = ", 4 * i + j - 1, 4 * i + j - 4);
				}
			}
			// Print the actual word byte by byte with a space
			sprintf(buffer + strlen(buffer), "%02x %02x %02x %02x ", wptr[3], wptr[2], wptr[1], wptr[0]);

			// Right side is not given on the last run, so i = 10
			// So add new lines after printing each word
			if (i == 10) {
				sprintf(buffer + strlen(buffer), "\n");
			}

			// Print the left side once its buffered
			fprintf(file, "%-32s", buffer);

			// Print the right side now
			if (i < 10) {
				// Switch case to handle which right side statement is printed based on what run this is
				switch (j) {
				case 0:
					wptr = (byte*)&x;
					fprintf(file, "| RotWord (w%d) = %02x %02x %02x %02x = x%d\n", 4 * i + j, wptr[3], wptr[2], wptr[1], wptr[0], i + 1);
					break;
				case 1:
					wptr = (byte*)&y;
					fprintf(file, "| SubWord (x%d) = %02x %02x %02x %02x = y%d\n", i + 1, wptr[3], wptr[2], wptr[1], wptr[0], i + 1);
					break;
				case 2:
					wptr = (byte*)&Rcon;
					fprintf(file, "| Rcon (%d) = %02x %02x %02x %02x\n", i + 1, wptr[3], wptr[2], wptr[1], wptr[0]);
					break;
				case 3:
					wptr = (byte*)&z;
					fprintf(file, "| y%d ^ Rcon (%d) = %02x %02x %02x %02x = z%d\n", i + 1, i + 1, wptr[3], wptr[2], wptr[1], wptr[0], i + 1);
					break;
				}
			}
		}
	}
	// Close the file since writing is done
	fclose(file);
}

// Functions for AES are below

// Uses s-box to substitute each byte
void SubBytes(byte state[]) {
	// For all 16 bytes...
	for (int i = 0; i < 16; i++) {
		// Get the indexes based on each byte
		int leftBits = state[i] >> 4;
		int rightBits = state[i] & 0x0f;

		// Replace the current byte with s-box byte
		state[i] = sbox[leftBits][rightBits];
	}
}

void ShiftRows(byte state[]) {
	byte backup[4];
	// For each row, shift i times in the ith row
	for (int i = 0; i < 4; i++) {
		for (int j = 0; j < 4; j++) {
			// Take a backup of each row for replacing bytes to complete circular shift
			backup[j] = state[i + 4 * j];
			// Ensure the intended byte to replace with is inside bounds of array
			if (i + (4 * j) + (4 * i) < 16) {
				// Replace the byte as a shift
				state[i + 4 * j] = state[i + (4 * j) + (4 * i)];
			}
		}
		// Once all initial shifts are completed and the backup is taken, replace the missing shifts (aka the cyclic ones)
		int k = 0;
		for (int j = i - 1; j >= 0; j--) {
			state[(12 + i) - (4 * j)] = backup[k++];
		}
	}
}

void MixColumns(byte state[]) {
	// Define the multiplication matrix, ordered in the same way as state (column first, then row)
	byte matrix[16] = { 0x02, 0x01, 0x01, 0x03, 0x03, 0x02, 0x01, 0x01, 0x01, 0x03, 0x02, 0x01, 0x01, 0x01, 0x03, 0x02 };

	for (int i = 0; i < 4; i++) {
		// Yse result to store the result after each column is solved
		byte result[4] = { 0, 0, 0, 0 };
		// Use byteResult to store each of the 4 different operations to reach the final value
		byte byteResult[4] = { 0, 0, 0, 0 };

		for (int j = 0; j < 4; j++) {
			// Solve for each byte in each column
			for (int k = 0; k < 4; k++) {
				// If multiplied by 1, then we simply xor to result
				if (matrix[j + 4 * k] == 0x01) {
					byteResult[k] = state[4 * i + k];
				}
				else if (matrix[j + 4 * k] == 0x02) {
					// If 2, we left shift by 1 bit
					byteResult[k] = state[4 * i + k] << 1;
					// If the leftmost bit was 1, we mod (xor) by m(x), or 0x1b as defined in AES
					if (state[4 * i + k] & 0x80) {
						byteResult[k] = byteResult[k] ^ 0x1b;
					}
				}
				else {
					// If 3, shift first (multiplying by x)
					byteResult[k] = state[4 * i + k] << 1;
					// Then check if we need to mod
					if (state[4 * i + k] & 0x80) {
						byteResult[k] = byteResult[k] ^ 0x1b;
					}
					// Then we xor with the original value (add 1 * stateByte for multiplying by x + 1)
					byteResult[k] = byteResult[k] ^ state[4 * i + k];
				}
			}
			result[j] = byteResult[0] ^ byteResult[1] ^ byteResult[2] ^ byteResult[3];
		}

		// Update the state
		state[4 * i] = result[0];
		state[4 * i + 1] = result[1];
		state[4 * i + 2] = result[2];
		state[4 * i + 3] = result[3];
	}
}

// Function to xor state with the round key
void AddRoundKey(byte state[], word w[], int round) {
	// Establish a pointer to the word array based on the current round
	byte *wptr = (byte*)w + (4 * (round + 1)) * sizeof(word);
	// Xor with the appropriate round key
	for (int i = 0; i < 4; i++) {
		state[4 * i] = state[4 * i] ^ wptr[3];
		state[4 * i + 1] = state[4 * i + 1] ^ wptr[2];
		state[4 * i + 2] = state[4 * i + 2] ^ wptr[1];
		state[4 * i + 3] = state[4 * i + 3] ^ wptr[0];
		wptr += sizeof(word);
	}
}

void WriteInitialRound(byte key[], byte plaintext[], FILE* file) {
	// Print plaintext and key at top
	fprintf(file, "Plaintext: ");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", plaintext[i]);
	}
	fprintf(file, "\n");

	fprintf(file, "Key: ");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", key[i]);
	}
	fprintf(file, "\n\n");

	// Create table headers
	fprintf(file, "%-14s|%-14s|%-15s|%-16s|%-14s\n", "Start of Round", "After SubBytes", "After ShiftRows", "After MixColumns", "Round Key");
	fprintf(file, "---------------------------------------------------------------------------\n");

	// Add initial rows of data
	for (int i = 0; i < 4; i++) {
		fprintf(file, " %02x %02x %02x %02x  |              |               |                | %02x %02x %02x %02x\n", plaintext[i], plaintext[i + 4], plaintext[i + 8], plaintext[i + 12], key[i], key[i + 4], key[i + 8], key[i + 12]);
	}

	fprintf(file, "---------------------------------------------------------------------------\n");
}

void WriteRound(byte initial[], byte sub[], byte shift[], byte mix[], word w[], int round, FILE* file) {
	byte *wptr = (byte*)w + (4 * (round + 1)) * sizeof(word);

	for (int i = 0; i < 4; i++) {
		// If this is the last round, we do not execute Mixcolumns so do not print it either
		if (round == 9) {
			fprintf(file, " %02x %02x %02x %02x  | %02x %02x %02x %02x  |  %02x %02x %02x %02x  |                | %02x %02x %02x %02x\n", initial[i], initial[i + 4], initial[i + 8], initial[i + 12],
				sub[i], sub[i + 4], sub[i + 8], sub[i + 12], shift[i], shift[i + 4], shift[i + 8], shift[i + 12], wptr[(3 - i)], wptr[(3 - i) + 4], wptr[(3 - i) + 8], wptr[(3 - i) + 12]);
		}
		// Otherwise print all data
		else {
			fprintf(file, " %02x %02x %02x %02x  | %02x %02x %02x %02x  |  %02x %02x %02x %02x  |  %02x %02x %02x %02x   | %02x %02x %02x %02x\n", initial[i], initial[i + 4], initial[i + 8], initial[i + 12],
				sub[i], sub[i + 4], sub[i + 8], sub[i + 12], shift[i], shift[i + 4], shift[i + 8], shift[i + 12], mix[i], mix[i + 4], mix[i + 8], mix[i + 12], wptr[(3 - i)], wptr[(3 - i) + 4], wptr[(3 - i) + 8], wptr[(3 - i) + 12]);
		}
	}

	fprintf(file, "---------------------------------------------------------------------------\n");
}

void WritePostRounds(byte ciphertext[], FILE* file) {
	// Print the ciphertext by itself to follow the table format
	for (int i = 0; i < 4; i++) {
		fprintf(file, " %02x %02x %02x %02x  |              |               |                |            \n", ciphertext[i], ciphertext[i + 4], ciphertext[i + 8], ciphertext[i + 12]);
	}
}

// Functions for Part 3 are contained here
int CompareBits(byte text1[], byte text2[]) {
	int bits = 0;
	for (int i = 0; i < 16; i++) {
		// This for loop will check every bit position in a given byte (text[i])
		for (byte b = 1; b != 0; b = b << 1) {
			// If the result of a bitwise and with b differs between the two, the bits are different and we add one to the counter
			if ((text1[i] & b) != (text2[i] & b)) bits++;
		}
	}
	return bits;
}

void WriteInitialComparison(byte textBase[], byte text[], byte stateBase[], byte state[], byte key[], FILE* file) {
	// Print input values first
	fprintf(file, "Baseline plaintext:  ");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", textBase[i]);
	}
	fprintf(file, "\n");

	fprintf(file, "Differing plaintext: ");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", text[i]);
	}
	fprintf(file, "\n\n");

	fprintf(file, "Key: ");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", key[i]);
	}
	fprintf(file, "\n\n");

	// Now print table headers
	fprintf(file, "Round|                                |Number of Bits that Differ\n");
	fprintf(file, "-----------------------------------------------------------------\n");

	// Now print initial texts
	fprintf(file, "     |");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", textBase[i]);
	}
	fprintf(file, "| %d\n", CompareBits(textBase, text));

	fprintf(file, "     |");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", text[i]);
	}
	fprintf(file, "| \n");

	fprintf(file, "-----------------------------------------------------------------\n");

	// Next, print after first key xor
	fprintf(file, "  0  |");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", stateBase[i]);
	}
	fprintf(file, "| %d\n", CompareBits(stateBase, state));

	fprintf(file, "     |");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", state[i]);
	}
	fprintf(file, "| \n");
}

void WriteRoundComparison(byte stateBase[], byte state[], int round, FILE* file) {
	fprintf(file, "-----------------------------------------------------------------\n");

	// Write the comparison of the results of both rounds
	// Simple if statement to make the formatting look correct for round number
	if (round < 9) {
		fprintf(file, "  %d  |", round + 1);
	}
	else {
		fprintf(file, " %d  |", round + 1);
	}
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", stateBase[i]);
	}
	fprintf(file, "| %d\n", CompareBits(stateBase, state));

	fprintf(file, "     |");
	for (int i = 0; i < 16; i++) {
		fprintf(file, "%02x", state[i]);
	}
	fprintf(file, "| \n");
}
