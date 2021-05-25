using Godot;
using System;

public class Account : CanvasLayer
{
    public override void _Ready()
    {
        GetNode<Log>("Log").HideWindow();
		if(UserAccounts.instance.currentUser != null) _OnLoggedIn();
    }
	
	private void _OnCreateButtonPressed() {
		GetNode<Log>("Log").ShowWindow(true);
	}
	
	private void _OnLogButtonPressed() {
		if(UserAccounts.instance.currentUser == null) GetNode<Log>("Log").ShowWindow(false);
		else {
			UserAccounts.instance.Logout();
			GetNode<Label>("LogLabel").Text = "Not currently logged in!";
			GetNode<Button>("LogButton").Text = "Log In";
			GetNode<Button>("CreateButton").Disabled = false;
		}
	}
	
	private void _OnMenuButtonPressed() {
		GetTree().ChangeScene("Menu.tscn");
		UserAccounts.instance.Log("Account>Menu button pressed.");
	}
	
	private void _OnLoggedIn() {
		GetNode<Label>("LogLabel").Text = "Currently logged in: " + UserAccounts.instance.currentUser.username;
		GetNode<Button>("LogButton").Text = "Log Out";
		GetNode<Button>("CreateButton").Disabled = true;
	}
}
