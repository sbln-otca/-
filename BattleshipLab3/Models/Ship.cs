namespace BattleshipLab3.Models;

public class Ship
{
    public int Length { get; }
    public bool IsVertical { get; private set; }
    public int StartX { get; private set; }
    public int StartY { get; private set; }
    public int Hits { get; private set; }

    public bool IsSunk => Hits >= Length;

    public Ship(int length, int startX, int startY, bool isVertical)
    {
        Length = length;
        StartX = startX;
        StartY = startY;
        IsVertical = isVertical;
    }

    public void Hit()
    {
        if (!IsSunk)
            Hits++;
    }
}

