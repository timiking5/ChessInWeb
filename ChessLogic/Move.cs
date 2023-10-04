namespace ChessLogic;

public class Move
{
    public int StartIndex { get; private set; }
    public int EndIndex { get; private set; }
    public int MoveFlag { get; private set; }
    public readonly struct Flag
    {
        public const int None = 0;
        public const int EnPassantCapture = 1;
        public const int Castling = 2;
        public const int PromoteToQueen = 3;
        public const int PromoteToKnight = 4;
        public const int PromoteToRook = 5;
        public const int PromoteToBishop = 6;
        public const int PawnTwoForward = 7;
    }
    public int PromotionPieceType
    {
        get
        {
            switch (MoveFlag)
            {
                case Flag.PromoteToRook:
                    return Piece.Rook;
                case Flag.PromoteToKnight:
                    return Piece.Knight;
                case Flag.PromoteToBishop:
                    return Piece.Bishop;
                case Flag.PromoteToQueen:
                    return Piece.Queen;
                default:
                    return Piece.None;
            }
        }
    }
    public Move(int startIndex, int endIndex, int moveFlag)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
        MoveFlag = moveFlag;
    }
    public Move(int startIndex, int endIndex)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
        MoveFlag = 0;
    }
}
