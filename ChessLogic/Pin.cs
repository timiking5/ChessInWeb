namespace ChessLogic;

public class Pin
{
    public int Square { get; set; }
    public int Direction { get; set; }
    public Pin(int square, int direction)
    {
        Square = square;
        Direction = direction;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pin pin &&
               Square == pin.Square &&
               Direction == pin.Direction;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Square, Direction);
    }
}
