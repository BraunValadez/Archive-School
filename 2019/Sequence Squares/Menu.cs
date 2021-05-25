using Godot;
using System;

public class Menu : CanvasLayer
{	
	// Pre-load the scenes for each of the buttons
	[Export]
	public PackedScene Main;
	
	[Export]
	public PackedScene Account;
	
	[Export]
	public PackedScene Scoreboard;
	
	// Function runs on press of Play Button
	private void _OnPlayButtonPressed()
	{
    	GetTree().ChangeSceneTo(Main);
		UserAccounts.instance.Log("Menu>Play button pressed.");
	}
	
	// Function runs on press of Account Button
	private void _OnAccountButtonPressed()
	{
    	GetTree().ChangeSceneTo(Account);
		UserAccounts.instance.Log("Menu>Account button pressed.");
	}
	
	// Function runs on press of Scoreboard Button
	private void _OnScoreButtonPressed()
	{
    	GetTree().ChangeSceneTo(Scoreboard);
		UserAccounts.instance.Log("Menu>Score button pressed.");
	}
	
	// Function runs on press of Quit Button
	private void _OnQuitButtonPressed()
	{
		// Exits the game
		UserAccounts.instance.Log("Menu>Quit button pressed.");
		UserAccounts.instance.Logout();
		GlobalScoreboard.instance.SaveFile();
    	GetTree().Quit();
	}
}
