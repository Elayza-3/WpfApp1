using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class TcpClient
    {
        private readonly Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public async Task ConnectAsync(string ipAddress, int port)
        {
            await _clientSocket.ConnectAsync(ipAddress, port);
            Task.Run(ReceiveMessagesAsync);
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            try
            {
                while (true)
                {
                    var bytesRead = await _clientSocket.ReceiveAsync(buffer, SocketFlags.None);
                    if (bytesRead == 0)
                        break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                 
                }
            }
            catch (SocketException)
            {
                // Server disconnected
            }
            finally
            {
                _clientSocket.Close();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _clientSocket.SendAsync(messageBytes, SocketFlags.None);
        }

        public void Disconnect()
        {
            _clientSocket.Close();
        }
    }
}