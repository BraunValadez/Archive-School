using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Main : Node
{
	// Array of colors for the squares
	private String[] _colors = { "blue", "brown", "cyan", "green", "orange", "pink", "purple", "red", "yellow" };
	
	// Array of current square objects
	private RigidBody2D[] _currentSquares = new RigidBody2D[9];
	
	// List containing the sequence and reverse order (specifically the index of the square from the array)
	private List<int> _sequence = new List<int>();
	private List<int> _reverseSequence = new List<int>();
	
	// Pre-load the scenes dedicated to the squares and menu
	[Export]
	public PackedScene SquareScene;
	
	// Signal to delete old squares
	[Signal]
	public delegate void RemoveSquares();
	
	// Make random number generator for random positions
	private Random _random = new Random();
	
	// Holds screen size
	private Vector2 _screenSize;
	
	// Score variable - specifically for keeping track of how long the sequence is
	private int _score;
	
	// Variable for tracking player's current place in sequence (-1 if not in game as to not register clicks)
	public int _sequenceNum {private set; get;}
	
	// Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		// Sets a Vector2 with sizes of screen
        _screenSize = GetViewport().GetSize();
		// Sets score to an initial value of 1 (which gets incremented on game start to 2, the actual starting value)
		_score = 1;
		_sequenceNum = -1;
    }
	
	public void NewRound() {
		// Emit signal to squares to remove themselves and clean up arrays/lists
		Array.Clear(_currentSquares, 0, _currentSquares.Length);
		_sequence.Clear();
		EmitSignal("RemoveSquares");
		
		// Increase score/sequence count by one, and set sequenceNum to -1, so the game will not register before it's ready
		_score++;
		_sequenceNum = -1;
		
		// Iterate through the array to spawn all 9 squares of each color
		for(int i = 0; i < _colors.Length; i++ ) {
			// Create an instance of the square scene and add it to scene tree
			var squareInstance = (Square)SquareScene.Instance();
			AddChild(squareInstance);
			squareInstance._mainInstance = this;
			// Set its animation (which is how colors are assigned in this case)
			squareInstance.GetNode<AnimatedSprite>("AnimatedSprite").Animation = _colors[i];
			// Set a random position between 2, (max x - 2) - constraint is to avoid them spawning off screen
			// Squares will not overlap since they have physics applied to them, and will push each other out of the way
			squareInstance.Position = new Vector2((float)_random.NextDouble() * (_screenSize.x - 4) + 2, (float)_random.NextDouble() * (_screenSize.y - 4) + 2);
			// Connect the signal RemoveSquares to the new square instance, so it can listen for the cue to remove itself
			Connect("RemoveSquares", squareInstance, "Delete");
			// Connect signals so any time a square is clicked, it calls OnSquareClicked
			squareInstance.Connect("SquareClicked", this, "_OnSquareClicked");
			// Add to an array of current squares
			_currentSquares[i] = squareInstance;
		}
		
		// Create the sequence from the array of current squares
		for (int i = 0; i < _score; i++) {
			_sequence.Add(_random.Next(0, 9));
		}
		
		// Create reverse sequence
		_reverseSequence = _sequence.ToList();
		_reverseSequence.Reverse();
		
		// Start the timer, which is to delay the start of the flashing squares
		GetNode<Timer>("StartTimer").Start();
	}
	
	public void _OnSquareClicked(RigidBody2D instance) {
		// If game is not ready to process input, return
		if(_sequenceNum == -1) return;
		
		// Check if the button pressed is equal to the one in either of the sequences
		if(Array.IndexOf(_currentSquares, instance) == _sequence[_sequenceNum] || Array.IndexOf(_currentSquares, instance) == _reverseSequence[_sequenceNum]) {
			// Increment the sequence number (we do this here because it will be equal to the count of the list (last index + 1)
			_sequenceNum++;
			// If you've made it through the sequence - you advance to the next round!
			if(_sequenceNum == _sequence.Count) { 
				// Alert the player that they've won, and allow them to click the button
				// The names of the nodes here look like directories since the nodes we need are in another scene, the "Ready" scene
				// Also set sequenceNum back to -1 so no more clicks are registered
				_sequenceNum = -1;
				UserAccounts.instance.Log("Round completed with sequence of " + _score + " squares.");
				GetNode<Label>("Ready/InstructionsLabel").Text = "You completed the round! Click the Continue button to go to the next round!";
				GetNode<Button>("Ready/ReadyButton").Text = "Continue";
				GetNode<Label>("Ready/InstructionsLabel").Show();
				GetNode<Button>("Ready/ReadyButton").Show();
			}
		// Otherwise, you've clicked the wrong one - you lose!
		} else {
			// Update score and bring up text with final score and the buttons to go back to menu or view scoreboard
			// Also set sequenceNum back to -1 so no more clicks are registered
			_sequenceNum = -1;
			_score--;
			// Check if user is logged in, and if so, add scores to history and check if new best score
			if(UserAccounts.instance.currentUser != null) {
				if(UserAccounts.instance.currentUser.bestScore < _score) {
					GlobalScoreboard.instance.AddScore(_score);
					UserAccounts.instance.currentUser.bestScore = _score;
					UserAccounts.instance.Log("New best score!");
				}
				// Add scores to history, and sort them in order (for easier printing in the scoreboard)
				UserAccounts.instance.currentUser.scoreHistory.Add(new Tuple<DateTime, int>(DateTime.Now, _score));
				UserAccounts.instance.currentUser.scoreHistory = UserAccounts.instance.currentUser.scoreHistory.OrderByDescending(t => t.Item2).ThenByDescending(t => t.Item1).ToList();
				UserAccounts.instance.Log("Finished with score: " + _score);
			}
			GetNode<Label>("Lost/ScoreLabel").Text = "Score: " + (_score);
			GetNode<Label>("Lost/LostLabel").Show();
			GetNode<Label>("Lost/ScoreLabel").Show();
			GetNode<Button>("Lost/MenuButton").Show();
			GetNode<Button>("Lost/ScoreButton").Show();
		}
	}
	
	// Executes after the "StartTimer" finishes counting down
	private async void _OnStartTimerTimeout() {
		// Log position and color of all squares
		StringBuilder sb = new StringBuilder();
		sb.Append("Square positions: ");
		for(int i = 0; i < _currentSquares.Length; i++) {
			sb.Append("[" + _colors[i] + "(" + _currentSquares[i].Position.x + ", " + _currentSquares[i].Position.y + ")] ");
		}
		UserAccounts.instance.Log(sb.ToString());
		sb.Clear();
		// Log correct sequence
		sb.Append("Correct sequence: ");
		sb.Append(_colors[_sequence[0]]);
		for(int i = 1; i < _sequence.Count; i++) {
			sb.Append(", " + _colors[_sequence[i]]);
		}
		UserAccounts.instance.Log(sb.ToString());
		// Iterate through the sequence list, having each square play the flash animation
		for(int i = 0; i < _sequence.Count; i++) {
			var sequenceNode = _currentSquares[_sequence[i]].GetNode<AnimatedSprite>("AnimatedSprite");
			sequenceNode.Play();
			// Wait for the animation to finish before playing the next one
			await ToSignal(sequenceNode, "animation_finished");
		}
		
		// Set the counter to 0
		_sequenceNum = 0;
	}
}
