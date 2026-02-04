using System.Text.Json.Serialization;
using BattleshipLab3.Models;

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

    // Флаг, что после этого выстрела у игрока, который ОТВЕЧАЕТ, не осталось кораблей.
    // Используется, чтобы стрелявший точно знал, что он победил.
    [JsonPropertyName("allSunk")]
    public bool? AllSunk { get; set; }

    // Обновления видимого состояния клеток (для поля противника)
    [JsonPropertyName("cells")]
    public List<CellUpdate>? Cells { get; set; }
}

public class CellUpdate
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; } = CellState.Empty.ToString();
}

