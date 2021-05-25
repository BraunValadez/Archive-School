using Godot;
using System;

public class Lost : CanvasLayer
{	
	// When menu button is pressed, go to menu
	private void _OnMenuButtonPressed() {
		GetTree().ChangeScene("Menu.tscn");
		UserAccounts.instance.Log("Lost>Menu button pressed.");
	}
	
	// When scoreboard button is pressed, go to scoreboard
	private void _OnScoreButtonPressed() {
		GetTree().ChangeScene("Scoreboard.tscn");
		UserAccounts.instance.Log("Lost>Score button pressed.");
	}
}
