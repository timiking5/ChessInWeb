using ChessLogic;

public class Runner
{
    public static void Main()
    {
        Board board = new();
        board.LoadPositionFromFen(Util.KnightCheckPos);
        MoveGenerator moveGen = new(board);
        var moves = moveGen.GenerateMoves();
    }
}