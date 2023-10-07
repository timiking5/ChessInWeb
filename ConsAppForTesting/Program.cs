using ChessLogic;

public class Runner
{
    public static void Main()
    {
        Board board = new();
        board.LoadPositionFromFen(Util.BishopCheckPos);
        MoveGenerator moveGen = new(board);
        var moves = moveGen.GenerateMoves();
    }
}