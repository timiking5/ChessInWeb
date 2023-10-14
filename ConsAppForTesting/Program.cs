using ChessLogic;

public class Runner
{
    public static void Main()
    {
        Board board = new();
        board.LoadPositionFromFen(Util.IllegalEnPassantPos2);
        board.EnPassantCol = 2;
        MoveGenerator moveGen = new(board);
        var moves = moveGen.GenerateMoves();
    }
}