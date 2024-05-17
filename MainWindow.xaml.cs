using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private TcpServer _server;
        private TcpClient _client;
        private string _userName;

        public MainWindow()
        {
            InitializeComponent();
            StartServerButton.Click += StartServerButton_Click;
            ConnectButton.Click += ConnectButton_Click;
            SendMessageButton.Click += SendMessageButton_Click;
            DisconnectButton.Click += DisconnectButton_Click;
            Closing += MainWindow_Closing;
        }

        private async void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateUserName())
            {
                _server = new TcpServer();
                _server.MessageReceived += OnMessageReceived;
                await _server.StartServerAsync(6464);
                UpdateChatLog("Server started.");
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateUserName() && ValidateIpAddress())
            {
                _client = new TcpClient();
                await _client.ConnectAsync(IPTextBox.Text, 6464);
                UpdateChatLog("Connected to server.");
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MessageTextBox.Text))
                return;

            string message = $"{_userName}: {MessageTextBox.Text}";
            if (_server != null)
            {
                _server.SendMessage(message);
            }
            else if (_client != null)
            {
                await _client.SendMessageAsync(message);
            }

            UpdateChatLog(message);
            MessageTextBox.Text = string.Empty;
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _server?.StopServer();
            _client?.Disconnect();
            UpdateChatLog("Disconnected.");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisconnectButton_Click(sender, null);
        }

        private void OnMessageReceived(object sender, string message)
        {
            UpdateChatLog(message);
        }

        private bool ValidateUserName()
        {
            if (string.IsNullOrEmpty(UserNameTextBox.Text) || UserNameTextBox.Text == "Enter Username")
            {
                MessageBox.Show("Please enter a valid username.");
                return false;
            }

            _userName = UserNameTextBox.Text;
            return true;
        }

        private bool ValidateIpAddress()
        {
            if (string.IsNullOrEmpty(IPTextBox.Text) || IPTextBox.Text == "Enter IP Address")
            {
                MessageBox.Show("Please enter a valid IP address.");
                return false;
            }

            return true;
        }

        private void UpdateChatLog(string message)
        {
            ChatListBox.Items.Add($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
            ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
        }
    }
}