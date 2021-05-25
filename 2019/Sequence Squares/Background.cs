using Godot;
using System;

public class Background : Node
{
	// Signal for removing squares in background
	[Signal]
	public delegate void RemoveBackgroundSquares();
	
	private String[] _colors = { "blue", "brown", "cyan", "green", "orange", "pink", "purple", "red", "yellow" };
	private Random _random = new Random();
	Vector2 _screenSize;
	
	// Pre-load the scenes dedicated to the squares
	[Export]
	public PackedScene SquareScene;
	
	public override void _Ready() {
		// Sets a Vector2 with sizes of screen
		_screenSize = GetViewport().GetSize();
		_OnBackgroundTimerTimeout();
	}
	
	// Once the timer depletes, spawn a new set of squares in the background
	private void _OnBackgroundTimerTimeout() {
		EmitSignal("RemoveBackgroundSquares");
		// Iterate through the array to spawn all 9 squares of each color
		for(int i = 0; i < _colors.Length; i++ ) {
			// Create an instance of the square scene and add it to scene tree
			var squareInstance = (Square)SquareScene.Instance();
			AddChild(squareInstance);
			// Set its animation (which is how colors are assigned in this case)
			squareInstance.GetNode<AnimatedSprite>("AnimatedSprite").Animation = _colors[i];
			// Set a random position between 2, (max x - 2) - constraint is to avoid them spawning off screen
			// Squares will not overlap since they have physics applied to them, and will push each other out of the way
			squareInstance.Position = new Vector2((float)_random.NextDouble() * (_screenSize.x - 4) + 2, (float)_random.NextDouble() * (_screenSize.y - 4) + 2);
			// Connect the signal RemoveSquares to the new square instance, so it can listen for the cue to remove itself
			Connect("RemoveBackgroundSquares", squareInstance, "Delete");
		}
	}
}
