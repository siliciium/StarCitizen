using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace SC_LogParser_Arena
{
    internal class WebSocketServer
    {
        private HttpListener _httpListener;
        private Dictionary<Guid, WebSocket> _clients = new Dictionary<Guid, WebSocket>();
        private volatile bool _isRunning;
        private static Dictionary<string, Dictionary<string, int>> _leaderboard;
        private static List<KeyValuePair<string, string>> _killfeed;
        private static System.Windows.Controls.Label _lstatus;
        private string _uriPrefix;

        public WebSocketServer(string uriPrefix, System.Windows.Controls.Label lstatus, Dictionary<string, Dictionary<string, int>> leaderboard, List<KeyValuePair<string, string>> killfeed)
        {
            _uriPrefix = uriPrefix;
            _lstatus = lstatus;
            _leaderboard = leaderboard;
            _killfeed = killfeed;            
        }

        public async Task StartAsync()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(_uriPrefix);

            _httpListener.Start();
            _isRunning = true;
            Debug.WriteLine("[WebSocketServer][StartAsync] " + _httpListener.Prefixes.FirstOrDefault());
            _lstatus.Content = "WebSocket server started at " + _httpListener.Prefixes.FirstOrDefault();

            while (_isRunning)
            {
                Debug.WriteLine("[WebSocketServer][StartAsync] Waiting new connection...");

                HttpListenerContext context = await _httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = webSocketContext.WebSocket;
                    Guid clientId = Guid.NewGuid();
                    _clients.Add(clientId, webSocket);


                    Debug.WriteLine($"[WebSocketServer][StartAsync][AcceptWebSocketAsync] {clientId}.");
                    HandleWebSocketConnection(clientId, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }                
            }

        }

        public void Stop()
        {
            _isRunning = false;

            Debug.WriteLine("[WebSocketServer][Stop]");
            foreach (var ws_client in _clients.Keys)
            {
                if (_clients[ws_client].State == WebSocketState.Open)
                {
                    Debug.WriteLine($"[WebSocketServer][Stop][CloseAsync][{ws_client.ToString()}]");
                    _clients[ws_client].CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).GetAwaiter().GetResult();
                }
            }
            _clients.Clear();
            
            _httpListener.Stop();
            Debug.WriteLine("[WebSocketServer][Stop][Stop]");
            _httpListener.Close();
            Debug.WriteLine("[WebSocketServer][Stop][Close]");
            
            _lstatus.Content = "WebSocket server stopped.";

        }


        private async void HandleWebSocketConnection(Guid clientId, WebSocket webSocket)
        {
            try
            {
                byte[] buffer = new byte[1024];

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.WriteLine($"[WebSocketServer][HandleWebSocketConnection] ws_cli {clientId} closed by the server.");
                        _clients.Remove(clientId);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the server", CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Debug.WriteLine($"[WebSocketServer][HandleWebSocketConnection][Rx] ws_cli {clientId} : {message}");

                        string jsonString = string.Empty;

                        try
                        {
                            switch (message)
                            {
                                case "/killfeed":
                                    {
                                        jsonString = JsonSerializer.Serialize(_killfeed, new JsonSerializerOptions { WriteIndented = true });
                                        break;
                                    }
                                case "/leaderboard":
                                    {
                                        jsonString = JsonSerializer.Serialize(_leaderboard, new JsonSerializerOptions { WriteIndented = true });
                                        break;
                                    }

                                default:
                                    {
                                        Debug.WriteLine($"Unrecognized command : {message}");
                                        jsonString = string.Empty;
                                        break;
                                    }
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                            _clients.Remove(clientId);
                        }
                        finally
                        {
                            if (!string.IsNullOrEmpty(jsonString))
                            {
                                string datas = string.Format("{{ \"{0}\" : {1} }}", message.Substring(1, message.Length - 1), jsonString);

                                byte[] responseBuffer = Encoding.UTF8.GetBytes(datas);
                                await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[WebSocketServer][HandleWebSocketConnection]");
                Debug.WriteLine(ex);
                Debug.WriteLine("[WebSocketServer][HandleWebSocketConnection] Removing wS_cli " + clientId);

                _clients.Remove(clientId);
                _lstatus.Content = "WebSocket server error occured.";
            }
        }

        
    }
}
