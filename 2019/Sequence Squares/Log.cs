using Godot;
using System;

public class Log : CanvasLayer
{
	// Signals for communicating with account scene to change the page slightly
	[Signal]
	public delegate void LoggedIn();
	
	private void _OnSubmitButtonPressed() {
		Label error = GetNode<Label>("ErrorLabel");
		LineEdit name = GetNode<LineEdit>("NameLineEdit");
		LineEdit pw = GetNode<LineEdit>("PasswordLineEdit");
		LineEdit confirm = GetNode<LineEdit>("ConfirmLineEdit");
		// First half (in if) is for creating an account. Else contains logging in
		if(GetNode<LineEdit>("ConfirmLineEdit").IsVisible()) {
			// Check if user file already exists
			if(UserAccounts.instance.CheckUserExists(name.Text)) {
				error.Text = "Error: Username already exists!";
				error.Show();
				return;
			}
			// Check if password and confirmation do not match
			if(pw.Text != confirm.Text) {
				error.Text = "Error: Passwords do not match.";
				error.Show();
				return;
			}
			// Else create the account and log in
			UserAccounts.instance.CreateUser(name.Text, pw.Text);
			EmitSignal("LoggedIn");
		} else {
			// Check if user file exists
			if(!(UserAccounts.instance.CheckUserExists(name.Text))) {
				error.Text = "Error: Username does not exist!";
				error.Show();
				return;
			}
			if (UserAccounts.instance.Login(name.Text, pw.Text)) {
				EmitSignal("LoggedIn");
			} else {
				error.Text = "Error: Incorrect password.";
				error.Show();
				return;
			}
		}
		// After logging in, hide the window
		HideWindow();
	}
	
	private void _OnBackButtonPressed() {
		HideWindow();
	}
	
	// Function that runs any time the password or name fields are changed
	private void _OnTextChanged(string x) {
		// If the confirm area is visible (then it's create an account window) check if all 3 text fields have something in them
		// If so, emable the submit button. Otherwise, disable it.
		if(GetNode<LineEdit>("ConfirmLineEdit").IsVisible()) {
			
			if (!String.IsNullOrEmpty(GetNode<LineEdit>("NameLineEdit").Text) && !String.IsNullOrEmpty(GetNode<LineEdit>("PasswordLineEdit").Text) && !String.IsNullOrEmpty(GetNode<LineEdit>("ConfirmLineEdit").Text))
				GetNode<Button>("SubmitButton").Disabled = false;
				
			else GetNode<Button>("SubmitButton").Disabled = true;
		} else { // If it's not a create account page, only check the username and password for text, and enable/disable the submit button
		
			if (!String.IsNullOrEmpty(GetNode<LineEdit>("NameLineEdit").Text) && !String.IsNullOrEmpty(GetNode<LineEdit>("PasswordLineEdit").Text))
				GetNode<Button>("SubmitButton").Disabled = false;
				
			else GetNode<Button>("SubmitButton").Disabled = true;
		}
	}
	
	// Function to make the window appear with argument for create an account vs log in
	public void ShowWindow(bool create) {
		// Text changes for the two seperate functions in here
		if (create) GetNode<Label>("TitleLabel").Text = "Create An Account";
		else GetNode<Label>("TitleLabel").Text = "Log In";
		
		// Actually show everything. Only show confirm password fields for creating an account
		GetNode<ColorRect>("ColorRect").Show();
		GetNode<Label>("TitleLabel").Show();
		GetNode<LineEdit>("NameLineEdit").Show();
		GetNode<LineEdit>("PasswordLineEdit").Show();
		if (create) GetNode<LineEdit>("ConfirmLineEdit").Show();
		GetNode<Label>("NameLabel").Show();
		GetNode<Label>("PasswordLabel").Show();
		if (create) GetNode<Label>("ConfirmLabel").Show();
		GetNode<Button>("SubmitButton").Show();
		GetNode<Button>("BackButton").Show();
	}
	
	// Function to easily clear and hide the entire window
	public void HideWindow() {
		GetNode<ColorRect>("ColorRect").Hide();
		GetNode<Label>("TitleLabel").Hide();
		GetNode<LineEdit>("NameLineEdit").Clear();
		GetNode<LineEdit>("PasswordLineEdit").Clear();
		GetNode<LineEdit>("ConfirmLineEdit").Clear();
		GetNode<LineEdit>("NameLineEdit").Hide();
		GetNode<LineEdit>("PasswordLineEdit").Hide();
		GetNode<LineEdit>("ConfirmLineEdit").Hide();
		GetNode<Label>("NameLabel").Hide();
		GetNode<Label>("PasswordLabel").Hide();
		GetNode<Label>("ConfirmLabel").Hide();
		GetNode<Label>("ErrorLabel").Hide();
		GetNode<Button>("SubmitButton").Hide();
		GetNode<Button>("BackButton").Hide();
	}
}
