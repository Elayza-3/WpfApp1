using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class TcpServer
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly List<Socket> _clients = new List<Socket>();
        private readonly object _lock = new object();

        public event EventHandler<string> MessageReceived;

        public async Task StartServerAsync(int port)
        {
            var ipAddress = IPAddress.Any;
            var listener = new TcpListener(ipAddress, port);
            listener.Start();

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    var client = await listener.AcceptSocketAsync();
                    lock (_lock)
                    {
                        _clients.Add(client);
                    }

                    Task.Run(() => HandleClientAsync(client), _cancellationTokenSource.Token);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private async Task HandleClientAsync(Socket client)
        {
            var buffer = new byte[1024];
            try
            {
                while (true)
                {
                    var bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (bytesRead == 0)
                        break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (SocketException)
            {
             
            }
            finally
            {
                lock (_lock)
                {
                    _clients.Remove(client);
                }
                client.Close();
            }
        }

        public void SendMessage(string message)
        {
            lock (_lock)
            {
                foreach (var client in _clients)
                {
                    client.Send(Encoding.UTF8.GetBytes(message));
                }
            }
        }

        public void StopServer()
        {
            _cancellationTokenSource.Cancel();
            lock (_lock)
            {
                foreach (var client in _clients)
                {
                    client.Close();
                }
                _clients.Clear();
            }
        }
    }
}