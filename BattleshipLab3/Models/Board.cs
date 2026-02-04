using System;

namespace BattleshipLab3.Models;

public class Board
{
    public const int Size = 10;

    private readonly CellState[,] _cells = new CellState[Size, Size];
    private readonly List<Ship> _ships = new();

    public CellState this[int x, int y] => _cells[x, y];

    public IReadOnlyList<Ship> Ships => _ships;

    public bool CanPlaceShip(int length, int startX, int startY, bool vertical)
    {
        for (int i = 0; i < length; i++)
        {
            int x = startX + (vertical ? 0 : i);
            int y = startY + (vertical ? i : 0);

            if (x < 0 || x >= Size || y < 0 || y >= Size)
                return false;

            if (!IsCellAndNeighboursEmpty(x, y))
                return false;
        }

        return true;
    }

    private bool IsCellAndNeighboursEmpty(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || nx >= Size || ny < 0 || ny >= Size)
                    continue;
                if (_cells[nx, ny] == CellState.Ship)
                    return false;
            }
        }

        return true;
    }

    public bool PlaceShip(int length, int startX, int startY, bool vertical)
    {
        if (!CanPlaceShip(length, startX, startY, vertical))
            return false;

        var ship = new Ship(length, startX, startY, vertical);
        _ships.Add(ship);

        for (int i = 0; i < length; i++)
        {
            int x = startX + (vertical ? 0 : i);
            int y = startY + (vertical ? i : 0);
            _cells[x, y] = CellState.Ship;
        }

        return true;
    }

    public void Clear()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                _cells[x, y] = CellState.Empty;
            }
        }

        _ships.Clear();
    }

    public void AutoPlaceStandardFleet()
    {
        Clear();
        var random = new Random();
        var lengths = new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

        foreach (var length in lengths)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < 1000)
            {
                attempts++;
                bool vertical = random.Next(2) == 0;
                int x = random.Next(Size);
                int y = random.Next(Size);
                placed = PlaceShip(length, x, y, vertical);
            }

            if (!placed)
                throw new InvalidOperationException("Не удалось расставить корабли.");
        }
    }

    public ShotResult ShootAt(int x, int y)
    {
        var current = _cells[x, y];
        if (current == CellState.Miss || current == CellState.Hit || current == CellState.Sunk)
            return new ShotResult(ShotOutcome.Repeated, null, new List<(int x, int y)>());

        if (current != CellState.Ship)
        {
            _cells[x, y] = CellState.Miss;
            return new ShotResult(ShotOutcome.Miss, null, new List<(int x, int y)>());
        }

        _cells[x, y] = CellState.Hit;

        var ship = FindShipAt(x, y);
        ship?.Hit();

        if (ship != null && ship.IsSunk)
        {
            var haloCells = MarkSunkShip(ship);
            return new ShotResult(ShotOutcome.Sunk, ship, haloCells);
        }

        return new ShotResult(ShotOutcome.Hit, ship, new List<(int x, int y)>());
    }

    private Ship? FindShipAt(int x, int y)
    {
        foreach (var ship in _ships)
        {
            for (int i = 0; i < ship.Length; i++)
            {
                int sx = ship.StartX + (ship.IsVertical ? 0 : i);
                int sy = ship.StartY + (ship.IsVertical ? i : 0);
                if (sx == x && sy == y)
                    return ship;
            }
        }

        return null;
    }

    private List<(int x, int y)> MarkSunkShip(Ship ship)
    {
        var halo = new List<(int x, int y)>();

        for (int i = 0; i < ship.Length; i++)
        {
            int x = ship.StartX + (ship.IsVertical ? 0 : i);
            int y = ship.StartY + (ship.IsVertical ? i : 0);
            _cells[x, y] = CellState.Sunk;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= Size || ny < 0 || ny >= Size)
                        continue;

                    if (_cells[nx, ny] == CellState.Empty)
                    {
                        _cells[nx, ny] = CellState.Miss;
                        halo.Add((nx, ny));
                    }
                }
            }
        }

        return halo;
    }

    public bool AllShipsSunk()
    {
        foreach (var ship in _ships)
        {
            if (!ship.IsSunk)
                return false;
        }

        return true;
    }
}

public record ShotResult(ShotOutcome Outcome, Ship? Ship, List<(int x, int y)> HaloCells);

public enum ShotOutcome
{
    Miss,
    Hit,
    Sunk,
    Repeated
}

