using Godot;
using System;

public class Scoreboard : CanvasLayer
{
	
	public override void _Ready() {
		// Store the global VerticalBox for easy access
		var globalContainer = GetNode<VBoxContainer>("GlobalScrollContainer/VBoxContainer");
		// If global sorebaord is empty, show placeholder text
		if(GlobalScoreboard.instance.scoreboard.Count == 0) {
			// Add label saying "No high scores" if no scores in the global scoreboard
			Label placeholder = new Label();
			globalContainer.AddChild(placeholder);
			placeholder.Align = Label.AlignEnum.Center;
			placeholder.Text = "No high scores.";
		} else {
			// Otherwise, print all high scores
			foreach(Tuple<string, int> t in GlobalScoreboard.instance.scoreboard) {
				Label newScore = new Label();
				globalContainer.AddChild(newScore);
				newScore.Align = Label.AlignEnum.Center;
				newScore.Text = t.Item1 + " - " + t.Item2;
			}
		}
		
		// Store the local VerticalBox for easy access
		var localContainer = GetNode<VBoxContainer>("LocalScrollContainer/VBoxContainer");
		// If user is not logged in, then don't show personal scores
		if(UserAccounts.instance.currentUser == null) {
			// Add label saying "Not logged in" if no user is logged in
			Label placeholder = new Label();
			localContainer.AddChild(placeholder);
			placeholder.Align = Label.AlignEnum.Center;
			placeholder.Text = "Not logged in.";
			// If they are logged in, check if they actually have scores or not
		} else if(UserAccounts.instance.currentUser.scoreHistory.Count == 0) {
			Label placeholder = new Label();
			localContainer.AddChild(placeholder);
			placeholder.Align = Label.AlignEnum.Center;
			placeholder.Text = "No logged scores.";
		} else {
			// Otherwise, print all their scores that are logged
			foreach(Tuple<DateTime, int> t in UserAccounts.instance.currentUser.scoreHistory) {
				Label newScore = new Label();
				localContainer.AddChild(newScore);
				newScore.Align = Label.AlignEnum.Center;
				newScore.Text = t.Item1.ToString("MM/dd/yy") + " - " + t.Item2;
			}
		}
	}
	
	private void _OnMenuButtonPressed() {
		GetTree().ChangeScene("Menu.tscn");
		UserAccounts.instance.Log("Scoreboard>Menu button pressed.");
	}
}
