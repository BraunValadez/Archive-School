using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class UserAccounts : Node
{
	public static UserAccounts instance;
	private const string _key = "C4t5M30w";
	
	// Make global current user and current history variables to make data sharing easier
	public AccountData currentUser;
	public List<string> currentHistory;
	
    public override void _Ready()
    {
        instance = this;
    }
	
	public void Log(string message) {
		if(currentHistory == null) return;
		Console.WriteLine("[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "] LOG: " + message); // *TODO: Maybe remove this or something later, when finished
		currentHistory.Add("[" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "] " + message);
	}
	
	// Function to check if a username is already taken (specifically by checking for a filename
	public bool CheckUserExists(string name) {
		return System.IO.File.Exists("users/" + name + ".usr");
	}
	
	// Function that actually creates the user account
	public void CreateUser(string name, string pw) {
		currentUser = new AccountData(name, pw);
		currentUser.bestScore = 0;
		currentHistory = new List<string>();
		Log("User account created.");
		// Adding currentHistory here is like adding a reference, so as currentHistory updates, it will be updated in allHistory as well
		currentUser.allHistory.Add(currentHistory);
		SaveFile();
	}
	
	// Function that encrypts data and then saves to a file in json format
	public void SaveFile() {
		// Convert the current user to the json format
		string json = JsonConvert.SerializeObject(currentUser);
		// Store envryption key in a byte array
		UnicodeEncoding ue = new UnicodeEncoding();
		byte[] bytes = ue.GetBytes(_key);
		// Make the file if it doesn't already exist and save it
		System.IO.Directory.CreateDirectory("users");
		FileStream fs = new FileStream("users/" + currentUser.username + ".usr", FileMode.Create);
		// Encrypt the file's data looping through the constant key above
		byte[] data = Encoding.UTF8.GetBytes(json);
		for(int i = 0; i < data.Length; i++) {
			fs.WriteByte(Convert.ToByte(data[i] + bytes[i % bytes.Length]));
		}
		// Close stream
		fs.Close();
	}
	
	public bool Login(string name, string pw) {
		// Convert encryption key to byte array
		UnicodeEncoding ue = new UnicodeEncoding();
		byte[] bytes = ue.GetBytes(_key);
		// Open stream from specified file
		FileStream fs = new FileStream("users/" + name + ".usr", FileMode.Open);
		// Create byte array for conversion, and read from file byte by byte, decrypt using same method as above, and add it to a list
		List<byte> data = new List<byte>();
		int byteInt, j = 0;
		// ReadByte returns an int, and returns -1 when failed, thus the check of "> 0"
		while((byteInt = fs.ReadByte()) > 0) {
			data.Add(Convert.ToByte(byteInt - bytes[j]));
			j++;
			if(j >= bytes.Length) j = 0;
		}
		// Once list is complete, convert to json format string and set currentUser to loaded data
		string json = Encoding.UTF8.GetString(data.ToArray());
		currentUser = JsonConvert.DeserializeObject<AccountData>(json);
		// Close stream
		fs.Close();
		// Create new history for this login session
		currentHistory = new List<string>();
		currentUser.allHistory.Add(currentHistory);
		// Convert the input password to md5 hash for checking later
		string pwmd5;
		using(MD5 md5 = MD5.Create()){
			byte[] pwdata = md5.ComputeHash(Encoding.UTF8.GetBytes(pw));
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < pwdata.Length; i++) {
				// Converts md5 has of password to hex (x = hex, 2 = 2 chars)
				sb.Append(pwdata[i].ToString("x2"));
			}
			pwmd5 = sb.ToString();
		}
		// Check if passwords match
		if(pwmd5 == currentUser.pwmd5) {
			Log("User logged in.");
			return true;
		} else { // If passwords do not match, it was a failed login attempt, so log that, and call "logout" with false as to not double-log things
			Log("Failed login attempt.");
			Logout(false);
			return false;
		}
	}
	
	// Log out and save data. Bool argument is to not add the user logging out if it was a failed login attempt, see above
	public void Logout(bool log = true) {
		// *TODO: Save scoreboard data
		if(log) Log("User logged out.");
		if (currentUser != null) SaveFile();
		currentUser = null;
		currentHistory = null;
	}
	
	// If the window is closed by other means, log the user out and log the action
	public override void _Notification(int what) {
		if (what == MainLoop.NotificationWmQuitRequest) {
			Log("Window closed.");
			Logout();
		}
	}
}

public class AccountData { 
	public string username;
	public string pwmd5;
	public int bestScore;
	public List<Tuple<DateTime, int>> scoreHistory;
	public List<List<string>> allHistory;
	
	// Constructor stores username/password
	public AccountData(string n, string pw) {
		username = n;
		// Use MD5 to store the password, and check for future login attempts
		using(MD5 md5 = MD5.Create()){
			byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(pw));
			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < data.Length; i++) {
				sb.Append(data[i].ToString("x2"));
			}
			pwmd5 = sb.ToString();
		}
		// Also create both lists so they are not null
		scoreHistory = new List<Tuple<DateTime, int>>();
		allHistory = new List<List<string>>();
	}
	// Blank default constructor for json
	public AccountData() { }
}