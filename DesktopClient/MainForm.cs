using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace DesktopClient
{
    public partial class MainForm : Form
    {
        private readonly HubConnection hubConnection;

        public MainForm()
        {
            InitializeComponent();

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/messages")
                .WithAutomaticReconnect()
                .ConfigureLogging(options =>
                {
                    options.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    options.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                    options.AddDebug();
                    options.AddSerilog();
                })
                .Build();

            hubConnection.On<NewMessage>("Send", message =>
            {
                AppendTextToTextBox(message.Sender, message.Text, Color.Black);
            });

            hubConnection.Closed += error =>
            {
                MessageBox.Show($"Connection closed. {error?.Message}");
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += id =>
            {
                MessageBox.Show($"Connection reconnected with id: {id}");
                return Task.CompletedTask;
            };

            hubConnection.Reconnecting += error =>
            {
                MessageBox.Show($"Connection reconnecting. {error?.Message}");
                return Task.CompletedTask;
            };
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await hubConnection.StartAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                if (hubConnection.State == HubConnectionState.Connected)
                {
                    connectButton.Text = "Disconnect";
                    stateLabelValue.ForeColor = Color.Green;
                    stateLabelValue.Text = "Connected";
                }
            }
            else if (hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.StopAsync();
                connectButton.Text = "Connect";
                stateLabelValue.ForeColor = Color.Red;
                stateLabelValue.Text = "Disconnected";
            }
        }

        private async void sendButton_Click(object sender, EventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {
                var message = new Message
                {
                    Text = messageTextBox.Text
                };

                try
                {
                    await hubConnection.SendAsync("SendToOthers", message);
                    AppendTextToTextBox("Me", message.Text, Color.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    messageTextBox.Clear();
                }
            }
        }

        private void AppendTextToTextBox(string sender, string text, Color color)
        {
            chatTextBox.SelectionStart = chatTextBox.TextLength;
            chatTextBox.SelectionLength = 0;
            chatTextBox.SelectionColor = color;
            chatTextBox.AppendText(string.Format("Author: {0}{2}Text: {1}{2}{2}", sender, text, Environment.NewLine));
            chatTextBox.SelectionColor = chatTextBox.ForeColor;
        }

        private async void setNameButton_Click(object sender, EventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    await hubConnection.SendAsync("SetMyName", nameTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    var name = await hubConnection.InvokeAsync<string>("GetMyName");

                    if (string.IsNullOrWhiteSpace(name))
                        nameTextBox.Text = "Anonymous";
                    else
                        nameTextBox.Text = name;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}