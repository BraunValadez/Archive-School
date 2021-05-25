using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Homework8 {
    public partial class ConnectForm : Form {
        public TcpClient client;
        public TcpListener host;

        public ConnectForm() {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e) {
            // If text box is empty, host
            // Also disable buttons to prevent weird double/lost connection issues
            buttonConnect.Enabled = false;
            textBoxIP.Enabled = false;
            if (textBoxIP.TextLength == 0) Host();
            else Connect(textBoxIP.Text);
        }

        private async Task Host() {
            textBoxLog.Text += "Hosting, listening for connection...\r\n";

            host = TcpListener.Create(25565);
            host.Start();

            client = await host.AcceptTcpClientAsync();
            host.Stop();

            textBoxLog.Text += "Connection received: " + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + "\r\n";
            textBoxLog.Text += "Starting game...\r\n";

            await Task.Delay(3000);
            Close();
        }

        private async Task Connect(string ip) {
            textBoxLog.Text += "Connecting to " + ip + "...\r\n";
            client = new TcpClient();
            bool connectionFailed = false;
            // Check if connection fails via timeout or other issue, say so below
            try {
                Task result = client.ConnectAsync(ip, 25565);
                if (result.Wait(5000)) {
                    textBoxLog.Text += "Connected.\r\n";
                    textBoxLog.Text += "Starting game...\r\n";

                    await Task.Delay(3000);
                    Close();
                } else {
                    textBoxLog.Text += "Connection timed out.\r\n";
                    connectionFailed = true;
                }
            } catch(Exception e) {
                connectionFailed = true;
            }
            if(connectionFailed) {
                textBoxLog.Text += "Connection failed.\r\n";
                buttonConnect.Enabled = true;
                textBoxIP.Enabled = true;
            }
            
        }
    }
}
