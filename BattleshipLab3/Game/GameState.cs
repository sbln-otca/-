using BattleshipLab3.Models;

namespace BattleshipLab3.Game;

public enum GamePhase
{
    WaitingForConnection,
    Placement,
    MyTurn,
    EnemyTurn,
    GameOver
}

public class GameState
{
    public Board MyBoard { get; } = new();
    public Board EnemyBoard { get; } = new();

    public GamePhase Phase { get; private set; } = GamePhase.WaitingForConnection;
    public bool IsHost { get; }

    public GameState(bool isHost)
    {
        IsHost = isHost;
    }

    public void SetPhase(GamePhase phase)
    {
        Phase = phase;
    }

    public void StartPlacement()
    {
        Phase = GamePhase.Placement;
    }

    public void StartGame(bool myTurnFirst)
    {
        Phase = myTurnFirst ? GamePhase.MyTurn : GamePhase.EnemyTurn;
    }

    public void SwitchTurn()
    {
        if (Phase == GamePhase.MyTurn)
            Phase = GamePhase.EnemyTurn;
        else if (Phase == GamePhase.EnemyTurn)
            Phase = GamePhase.MyTurn;
    }

    public void FinishGame()
    {
        Phase = GamePhase.GameOver;
    }
}

