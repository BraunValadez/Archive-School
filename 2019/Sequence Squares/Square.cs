using Godot;
using System;

public class Square : RigidBody2D
{
	// Basically if the mouse button is down last time it got an input event, this is true
	private bool _lastPressed = false;
	
	public Main _mainInstance {private get; set;}
	
	// Signal emitted once clicked
	[Signal]
	public delegate void SquareClicked(RigidBody2D instance);
	
	// Called when the square needs to destruct itself
	private void Delete() {
		QueueFree();
	}
	
	// Handles click events - more specifically makes sure it's a click (button down and up on the square instead of only one or the other)
	private void _OnSquareInputEvent(Node viewport, InputEvent e, int shape_idx) {
		if(_mainInstance == null || !(e is InputEventMouseButton) || _mainInstance._sequenceNum == -1) return;
		
		// Checks if the last time an event happened, the mosue button was held down, and for this event, the button is up (aka, a full click was completed)
		if(_lastPressed && !e.IsPressed()) {
			// Log color, position of clicked square
			UserAccounts.instance.Log("Main>Square clicked: " + GetNode<AnimatedSprite>("AnimatedSprite").Animation + ".");
			// Pass this Square object along with the signal
			EmitSignal("SquareClicked", this);
			// Play flash animation
			GetNode<AnimatedSprite>("AnimatedSprite").Play();
		}
		
		// Shows that the last time this was pressed, the mouse button was held down
		_lastPressed = e.IsPressed();
	}
	
	// Once animation is done, reset to first frame, and stop playing the animation
	private void _OnAnimationFinished() {
		var sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		sprite.Frame = 0;
		sprite.Stop();
	}
}
