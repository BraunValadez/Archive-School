using Godot;
using System;

public class Ready : CanvasLayer
{
	// Signal that's sent to main to start a round
	[Signal]
	public delegate void StartRound();
	
	// Function runs when Ready Button is pressed
	private void _OnReadyButtonPressed()
	{
		// Hide both label and button, leaving just the game screen
    	GetNode<Label>("InstructionsLabel").Hide();
		GetNode<Button>("ReadyButton").Hide();
		EmitSignal("StartRound");
		UserAccounts.instance.Log("Ready>Ready/Continue button pressed.");
	}
}
