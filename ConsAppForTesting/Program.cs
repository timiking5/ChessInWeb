using ChessLogic;

public class Runner
{
    public static void Main()
    {
        Board board = new();
        board.LoadPositionFromFen(Util.StartingPos);
        MoveGenerator moveGen = new(board);
        var moves = moveGen.GenerateMoves();
    }
}