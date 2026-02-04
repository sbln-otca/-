using System.Text.Json.Serialization;

namespace BattleshipLab3.Network;

public enum MessageType
{
    Connect,
    StartGame,
    Shot,
    ShotResult,
    Chat
}

public class NetworkMessage
{
    [JsonPropertyName("type")]
    public MessageType Type { get; set; }

    [JsonPropertyName("x")]
    public int? X { get; set; }

    [JsonPropertyName("y")]
    public int? Y { get; set; }

    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

