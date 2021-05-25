# Sequence Squares
This contains the project in which I created a simple game - Sequence Squares. In this game, the user is shown a screen with 9 randomly placed, different colored squares. A random sequence is chosen, and is shown to the user via the squares flashing. The user's goal is to then press the same sequence in either forward or reverse order - both are accepted and treated the same. The sequence starts with a length of 2, and increases by 1 after each successful completion. The game ends upon the user pressing an incorrect square, and their score is then saved to a leaderboard.

This project was created using Godot v3.1.1 using Godot's C# library: GodotSharp.

All source code files are included in this directory.

Since Godot is scene-based and each scene gets its own code file, below will be a list of the files and what each one covers. The files could coordinate between each other using a system called "Signals" in which a certain signal is sent, and any scene looking for a signal with that name will run the associated function. It is worth noting that individual entities are contained in their own scene, such as each square. To offer an example for the usage of signals: Main sends a signal "RemoveSquares" which tells all squares currently instantiated within Main to remove themselves, as the current round is over. The signal has the squares call their Delete function and they are removed from the scene.

**Scene Code Files:**
* Account.cs
	-This file handles simple events for loading the "Account" page and handling button click events.
* Background.cs
	-This file handles the effect on the main menu's background, in which squares will appear in random patterns after a set interval. This handles the code for sending signals to remove the squares, generate them and their positions.
* GlobalScoreboard.cs
	-This file contains the logic for loading and saving the scoreboard file. This also includes enryption and decryption. It's called "GlobalScoreboard" since we needed this code to execute globally, as we catch if the game exits at any point and save the scoreboard file before closing completely.
	
	-The scoreboard file is saved using a simple encryption method of reading the file byte-by-byte and adding the byte of the key. So the first byte of the file is added with the first byte in the key, the second in the file is added to the second in the key, and so on. Once the key is out of characters, the character used from the key is repeated. The process for decryption is similar, but you subtract the key as opposed to adding it.
* Log.cs
	-This file contains the logic for a user either logging in to their account or creating one. This primarily covers the displaying of certain labels based on if a user is logged in, as well as handles the logic for making sure a username is not already taken.
* Lost.cs
	-This file contains the logic for the displayed buttons after the user presses an incorrect square in the sequence. It only contains logic for redirecting the user based on button click events.
* Main.cs
	-This file handles the main game logic, including generating the squares, their positions, the sequence and the reverse sequence, showing that info to the user, and checks if the user's click on a square matches that in the sequence.
* Menu.cs
	-This file only contains logic for redirecting the user based on which button they click, as this is the main menu.
* Ready.cs
	-This file only contains logic for sending a "ready" signal to the main game scene.
* Scoreboard.cs
	-This file contains the logic for displaying the scoreboard as well as redirecting the user if they click on the menu button.
* Square.cs
	-This file contains the logic for each individual square, which is actually quite simple. It only handles emitting a signal if the square is clicked, or deleting itself.
* UserAccounts.cs
	-This file handles the logic for logging player actions as well as saving/loading the current user file. The encryption/decryption used is identical to the algorithm used in GlobalScoreboard.cs.
	
	-It is worth noting that both here, and in GlobalScoreboard.cs, the key is included in the source code. I am aware this is not secure and it should be in its own file, such as key.json. However, it was done this way due to both time constraints as well as slightly simpler access. If I planned on properly shipping this game, I would have used a more proper security method for this key. Also, at the time, I had very little knowledge of cryptography.

**Additional Notes**

The user files and scoreboard file used in testing is included in the "users" directory.

There are screenshots of the game available to view in the "screenshots" directory.

The only screenshots included below is the main menu.

![Game Main Menu](https://github.com/GloaNeko/Archive-School/blob/main/2019/Sequence%20Squares/screenshots/menu.png)
