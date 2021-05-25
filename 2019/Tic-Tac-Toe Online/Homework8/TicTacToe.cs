using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Homework8 {
    public partial class TicTacToe : Form {
        TcpClient client;
        NetworkStream stream;
        Button[] boardArray;
        string player;
        string opponent;

        public TicTacToe() {
            InitializeComponent();
            boardArray = new Button[] {button1, button2, button3, button4, button5, button6, button7, button8, button9};
        }

        // Function runs after any available space is clicked
        private void board_Click(object sender, EventArgs e) {
            // Edit the buttons client-side
            Button b = sender as Button;
            b.Text = player;

            // Send message of what button changed
            // SendMessage returns true if it was successful
            if (SendMessage(Array.IndexOf(boardArray, b).ToString())) {
                // See if the game is finished, label appropriate winner, draw or continue
                int check = CheckFinished();
                if(check != 0) {
                    stream.Close();
                    client.Close();
                    if (check == 1) labelInfo.Text = "You win";
                    else if (check == -1) labelInfo.Text = "You lose";
                    else if (check == -7) labelInfo.Text = "Draw";
                    buttonConnect.Text = "Connect";
                } else labelInfo.Text = "Opponent's turn";
            }

            // Disable buttons to prevent sending multiple turns
            foreach (Button btn in boardArray) {
                btn.Enabled = false;
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e) {
            // Become "Disconnect" button after connecting
            if (client != null && client.Connected) {
                buttonConnect.Text = "Connect";
                labelInfo.Text = "Disconnected";
                // Send a disconnect special message
                SendMessage("-1");
                stream.Close();
                client.Close();
            } else {
                // Open other window to handle connecting
                ConnectForm connectForm = new ConnectForm();
                connectForm.ShowDialog();
                client = connectForm.client;
                // If client wasn't successfully established, return
                if (client == null || !client.Connected) return;

                // Otherwise, change to disconnect button, reset board, and start game
                buttonConnect.Text = "Disconnect";
                foreach (Button b in boardArray) {
                    b.Text = "";
                    b.Enabled = (connectForm.host == null); // If you are not the host, buttons are enabled by default as it is your turn
                }
                if (connectForm.host == null) {
                    player = "X";
                    opponent = "O";
                    labelInfo.Text = "Your turn";
                }
                else {
                    player = "O";
                    opponent = "X";
                    labelInfo.Text = "Opponent's turn";
                }

                GameHandler();
            }
        }

        private async Task GameHandler() {
            Byte[] bytes = new byte[256];
            string data = null;
            stream = client.GetStream();

            while(true) {
                // Decode message into data string and then index int (called so because of array storage of buttons)
                int messageLength = await stream.ReadAsync(bytes, 0, bytes.Length);
                data = Encoding.ASCII.GetString(bytes, 0, messageLength);
                int index = int.Parse(data);

                // If message is -1, that means a disconnect was issued
                if (index == -1) {
                    stream.Close();
                    client.Close();

                    labelInfo.Text = "Disconnected";
                    buttonConnect.Text = "Connect";
                    foreach(Button b in boardArray) {
                        b.Enabled = false;
                    }
                } // Otherwise update board, check if the game has finished and change messages appropriately
                else {
                    boardArray[index].Text = opponent;
                    int check = CheckFinished();
                    if (check != 0) {
                        stream.Close();
                        client.Close();
                        if (check == 1) labelInfo.Text = "You win";
                        else if (check == -1) labelInfo.Text = "You lose";
                        else if (check == -7) labelInfo.Text = "Draw";
                        buttonConnect.Text = "Connect";
                    } // If not finished, continue playing
                    else {
                        labelInfo.Text = "Your turn";

                        foreach (Button b in boardArray) {
                            if (b.Text.Length == 0) {
                                b.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        private void TicTacToe_FormClosing(object sender, EventArgs e) {
            // Send a disconnect special message
            if(client != null && client.Connected) SendMessage("-1", false);
            // Close connection if not already
            stream?.Close();
            client?.Close();
        }

        // Function used for sending any time of message, returns bool to indicate success in some cases
        private bool SendMessage(string msg, bool reportError=true) {
            // Encode into byte array and send
            try {
                Byte[] bytes = Encoding.ASCII.GetBytes(msg);
                stream.Write(bytes, 0, bytes.Length);
            } // If connection is interrupted, do this:
            catch (System.IO.IOException ex) {
                if (reportError) {
                    // Error message
                    System.Media.SystemSounds.Beep.Play();
                    labelInfo.Text = "Network error: Disconnected";
                    // Close connection and lock the board
                    stream.Close();
                    client.Close();
                }
                return false;
            }
            return true;
        }

        private int CheckFinished() {
            string winner = null;

            // Check rows
            for(int i = 0; i < 9; i += 3) {
                if (boardArray[i].Text == boardArray[i + 1].Text && boardArray[i].Text == boardArray[i + 2].Text) winner = boardArray[i].Text;
            }
            // Check columns
            for(int i = 0; i < 3; i++) {
                if (boardArray[i].Text == boardArray[i + 3].Text && boardArray[i].Text == boardArray[i + 6].Text) winner = boardArray[i].Text;
            }
            // Check diagonals
            if (boardArray[2].Text == boardArray[4].Text && boardArray[2].Text == boardArray[6].Text) winner = boardArray[2].Text;
            if (boardArray[0].Text == boardArray[4].Text && boardArray[0].Text == boardArray[8].Text) winner = boardArray[0].Text;

            // Return int result of victor
            if (winner == player) return 1;
            if (winner == opponent) return -1;

            // Check if all boxes are filled (if so, means draw)
            foreach(Button b in boardArray) {
                if (b.Text.Length == 0) return 0;
            }
            // If draw, return -7 as a "special" value
            return -7;
        }
    }
}
