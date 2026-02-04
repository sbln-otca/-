using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BattleshipLab3.Network;

public class NetworkSession
{
    private readonly bool _isHost;
    private readonly int _port;
    private readonly string _hostAddress;

    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    public event Action<NetworkMessage>? MessageReceived;
    public event Action<string>? ConnectionStatusChanged;

    public NetworkSession(bool isHost, string hostAddress, int port)
    {
        _isHost = isHost;
        _hostAddress = hostAddress;
        _port = port;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();

        if (_isHost)
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            ConnectionStatusChanged?.Invoke("Ожидание подключения...");
            _client = await _listener.AcceptTcpClientAsync(_cts.Token);
            ConnectionStatusChanged?.Invoke("Клиент подключился");
        }
        else
        {
            _client = new TcpClient();
            ConnectionStatusChanged?.Invoke("Подключение к хосту...");
            await _client.ConnectAsync(_hostAddress, _port);
            ConnectionStatusChanged?.Invoke("Подключено к хосту");
        }

        _stream = _client.GetStream();
        _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    public async Task SendAsync(NetworkMessage message)
    {
        if (_stream == null)
            return;

        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        if (_stream == null)
            return;

        var buffer = new byte[4096];
        var builder = new StringBuilder();

        try
        {
            while (!token.IsCancellationRequested)
            {
                int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (read == 0)
                    break;

                builder.Append(Encoding.UTF8.GetString(buffer, 0, read));

                while (true)
                {
                    var text = builder.ToString();
                    int newlineIndex = text.IndexOf('\n');
                    if (newlineIndex < 0)
                        break;

                    var line = text[..newlineIndex].Trim();
                    builder.Remove(0, newlineIndex + 1);

                    if (line.Length == 0)
                        continue;

                    try
                    {
                        var msg = JsonSerializer.Deserialize<NetworkMessage>(line);
                        if (msg != null)
                            MessageReceived?.Invoke(msg);
                    }
                    catch
                    {
                        ConnectionStatusChanged?.Invoke("Ошибка чтения сообщения");
                    }
                }
            }
        }
        catch
        {
            ConnectionStatusChanged?.Invoke("Соединение разорвано");
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _stream?.Close();
        _client?.Close();
        _listener?.Stop();
    }
}

