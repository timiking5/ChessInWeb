namespace ChessLogic;

public class MoveGenerator
{
    private Board board;
    private bool whiteToMove;
    private bool inCheck;
    private bool inDoubleCheck;
    public MoveGenerator(Board board)
    {
        this.board = board;
        whiteToMove = board.WhiteToMove;
    }
    public List<Move> GenerateMoves()
    {
        inCheck = false;
        inDoubleCheck = false;
        HashSet<int> attackMask = CreateAttackMask();

        List<Move> moves = new();
        return moves;
    }

    private HashSet<int> CreateAttackMask()
    {
        HashSet<int> attackMap = new();
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        int attackingPiecesInd = whiteToMove ? 1 : 0;
        int attackingColour = whiteToMove ? 16 : 8;
        int startIndex = 0, endIndex = 8;
        if (board.Bishops[attackingPiecesInd].Count == 0 && board.Queens[attackingPiecesInd].Count == 0)
        {
            // no pieces can attack diagnoly
            endIndex = 4;
        }
        if (board.Rooks[attackingPiecesInd].Count == 0 && board.Queens[attackingPiecesInd].Count == 0)
        {
            // no pieces can attack in straight line
            startIndex = 4;
        }
        for (int i = startIndex; i < endIndex; i++)
        {
            int direction = Board.SlidingDirections[i];
            int friendlyPieces = 0;  // pieces in the way
            int square = kingSquare;
            HashSet<int> attackedSquares = new();
            while (square < 64)
            {

            }
        }
        return attackMap;
    }
}
