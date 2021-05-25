using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class GlobalScoreboard : Node
{
	// Make instance for calling in the scoreboard scene, the encryption key, as well as the list itself
	public static GlobalScoreboard instance;
	private const string _key = "C4t5M30w";
	public List<Tuple<string, int>> scoreboard;
	
	public override void _Ready() {
		instance = this;
		// Check if file exists, otherwise load it and store it to the list
		if(!(System.IO.File.Exists("users/Scoreboard.dat"))) scoreboard = new List<Tuple<string, int>>();
		else {
			// Convert encryption key to byte array
			UnicodeEncoding ue = new UnicodeEncoding();
			byte[] bytes = ue.GetBytes(_key);
			// Open stream from specified file
			FileStream fs = new FileStream("users/Scoreboard.dat", FileMode.Open);
			// Create byte array for conversion, and read from file byte by byte, decrypt file using reverse of save algorithm, and add it to a list
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
			scoreboard = JsonConvert.DeserializeObject<List<Tuple<string, int>>>(json);
			// Close stream
			fs.Close();
		}
	}
	
	// Function to add new high score to the global scoreboard
	public void AddScore(int score) {
		// Grab username (no need to check for currentUser since it will be done before call)
		string username = UserAccounts.instance.currentUser.username;
		// Search list for the current user's username, and remove their old score if its there
		foreach(Tuple<string, int> t in scoreboard.Where(o => o.Item1 == username)) {
			scoreboard.Remove(t);
		}
		// Add the new high score, and sort the list so that the highest score is at the top
		scoreboard.Add(new Tuple<string, int>(username, score));
		scoreboard.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));
	}
	
	// Function that encrypts data and then saves to a file in json format
	public void SaveFile() {
		// Convert the current user to the json format
		string json = JsonConvert.SerializeObject(scoreboard);
		// Store envryption key in a byte array
		UnicodeEncoding ue = new UnicodeEncoding();
		byte[] bytes = ue.GetBytes(_key);
		// Make the file if it doesn't already exist and save it
		System.IO.Directory.CreateDirectory("users");
		FileStream fs = new FileStream("users/Scoreboard.dat", FileMode.Create);
		// Encrypt the file using the constant key defined above
		byte[] data = Encoding.UTF8.GetBytes(json);
		for(int i = 0; i < data.Length; i++) {
			fs.WriteByte(Convert.ToByte(data[i] + bytes[i % bytes.Length]));
		}
		// Close stream
		fs.Close();
	}
	
	// If the window is closed by other means, save the scoreboard file
	public override void _Notification(int what) {
		if (what == MainLoop.NotificationWmQuitRequest) SaveFile();
	}
}
