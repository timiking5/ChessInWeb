using System.Reflection;

namespace ChessLogic;

public class Board
{
    /// <summary>
    /// The idea is not to use a 2d array, but to enumerate each square
    /// from 0 to 63. It has some benefits and working with it is not
    /// that complicated because it doesn't make piece moving harder.
    /// (You can check that yourself)
    /// </summary>
    public int[] Squares { get; private set; } = new int[64];
    public bool WhiteToMove;
    public double FiftyMoveCounter { get; set; } = 0;
    public int PlysCounter { get; set; } = 0;
    public int[] KingSquares { get; set; }
    public bool WhiteCanCastleKing { get; set; }
    public bool WhiteCanCastleQueen { get; set; }
    public bool BlackCanCastleKing { get; set; }
    public bool BlackCanCastleQueen { get; set; }
    public int ColourToMoveIndex { get; set; }
    public PieceList[] Rooks { get; set; }
    public PieceList[] Bishops { get; set; }
    public PieceList[] Queens { get; set; }
    public PieceList[] Knights { get; set; }
    public PieceList[] Pawns { get; set; }
    public int EnPassantCol = -1;  // tells if there is an opportunity to En Passant
    private static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>
    {
        ['k'] = Piece.King,
        ['q'] = Piece.Queen,
        ['n'] = Piece.Knight,
        ['p'] = Piece.Pawn,
        ['b'] = Piece.Bishop,
        ['r'] = Piece.Rook
    };
    public static int[] SlidingDirections = new int[] { -1, 1, -8, 8, -9, 9, -7, 7 };
    public static int[] KnightMoves = new int[] { -17, 17, -15, 15, -6, 6, -10, 10 };

    public void LoadPositionFromFen(string fen)
    {
        string[] meta = fen.Split(' ');
        string fenBoard = meta[0];
        int col = 0, row = 7;
        foreach (var ch in fenBoard)
        {
            if (ch == '/')
            {
                col = 0;
                row--;
                continue;
            }
            if (Char.IsDigit(ch))
            {
                col += int.Parse(ch.ToString());
                continue;
            }
            var piece = Char.ToLower(ch);
            int colour = Char.IsUpper(ch) ? Piece.White : Piece.Black;
            int colourIndex = colour / 16;
            Squares[col + row * 8] = colour + pieceTypeFromSymbol[piece];  // put a piece on board
            switch (piece)
            {
                case 'k':
                    KingSquares[colourIndex] = col + row * 8;
                    break;
                case 'q':
                    Queens[colourIndex].AddPieceAtSquare(col + row * 8);
                    break;
                case 'n':
                    Knights[colourIndex].AddPieceAtSquare(col + row * 8);
                    break;
                case 'b':
                    Bishops[colourIndex].AddPieceAtSquare(col + row * 8);
                    break;
                case 'r':
                    Rooks[colourIndex].AddPieceAtSquare(col + row * 8);
                    break;
                case 'p':
                    Pawns[colourIndex].AddPieceAtSquare(col + row * 8);
                    break;
                default:
                    break;
            }
            col++;
        }
        ExtractMetaData(meta);
    }

    private void ExtractMetaData(string[] meta)
    {
        WhiteToMove = meta[1] == "w";
        FiftyMoveCounter = int.Parse(meta[4]);
        PlysCounter = (int.Parse(meta[5]) - 1) * 2;
        PlysCounter += WhiteToMove ? 0 : 1;
        bool enPassant = meta[3] != "-";
        EnPassantCol = enPassant ? meta[3][0] - 'a' : -1;
        WhiteCanCastleKing = meta[2].Contains('K');
        WhiteCanCastleQueen = meta[2].Contains('Q');
        BlackCanCastleKing = meta[2].Contains('k');
        BlackCanCastleQueen = meta[2].Contains('q');
    }

    public Board()
    {
        KingSquares = new int[2];
        Knights = new PieceList[] { new PieceList(10), new PieceList(10) };
        Pawns = new PieceList[] { new PieceList(8), new PieceList(8) };
        Rooks = new PieceList[] { new PieceList(10), new PieceList(10) };
        Bishops = new PieceList[] { new PieceList(10), new PieceList(10) };
        Queens = new PieceList[] { new PieceList(9), new PieceList(9) };
    }
    public void MakeMove(Move move)
    {
        WhiteToMove = !WhiteToMove;
        bool fmc = false;  // fifty moves counter
        switch (move.MoveFlag)
        {
            case 0:
            case 1:
            case 7:
                fmc = MakeMoveUtil(move);
                break;
            case 2:
                CastleKing(move);
                break;
            case 3:
            case 4:
            case 5:
            case 6:
                PromotePawn(move);
                fmc = true;
                break;
        }
        PlysCounter++;
        FiftyMoveCounter = fmc ? 0 : FiftyMoveCounter + 0.5;
    }

    private void PromotePawn(Move move)
    {
        int index = WhiteToMove ? 0 : 1;
        Pawns[index].RemovePieceAtSquare(move.StartIndex);
        int newPiece = Piece.Colour(Squares[move.StartIndex]);
        Squares[move.StartIndex] = 0;
        int targetSquare = move.EndIndex;
        if (Squares[targetSquare] != Piece.None)
        {
            RemovePiece(Squares[targetSquare], targetSquare);
        }
        switch (move.MoveFlag)
        {
            case 3:
                Knights[index].AddPieceAtSquare(move.EndIndex);
                newPiece += 3;
                break;
            case 5:
                Bishops[index].AddPieceAtSquare(move.EndIndex);
                newPiece += 5;
                break;
            case 6:
                Rooks[index].AddPieceAtSquare(move.EndIndex);
                newPiece += 6;
                break;
            case 7:
                Queens[index].AddPieceAtSquare(move.EndIndex);
                newPiece += 7;
                break;
        }
        Squares[move.EndIndex] = newPiece;
    }

    private void CastleKing(Move move)
    {
        int offset = move.StartIndex == 4 ? 0 : 56;
        int rookSquare = move.EndIndex == 6 + offset ? 7 + offset : 0 + offset;
        int rookMoveSquare = move.EndIndex == 6 + offset ? 5 + offset : 3 + offset;
        int index = Piece.Colour(Squares[rookSquare]) == 8 ? 0 : 1;
        KingSquares[index] = move.EndIndex;
        Squares[move.EndIndex] = Squares[move.StartIndex];
        Squares[move.StartIndex] = 0;
        MovePiece(Squares[rookSquare], new(rookSquare, rookMoveSquare));
        switch (index)
        {
            case 0:
                WhiteCanCastleKing = false;
                WhiteCanCastleQueen = false;
                break;
            case 1:
                BlackCanCastleKing = false;
                BlackCanCastleQueen = false;
                break;
        }
    }

    private bool MakeMoveUtil(Move move)
    {
        int targetSquare = move.MoveFlag == 1 ? move.EndIndex - 8 : move.EndIndex;
        bool fmc = Piece.IsPawn(Squares[move.StartIndex]);
        if (Squares[targetSquare] != Piece.None)
        {
            RemovePiece(Squares[targetSquare], targetSquare);
            fmc = true;
        }
        MovePiece(Squares[move.StartIndex], move);
        if (move.MoveFlag == 7)
        {
            EnPassantCol = move.StartIndex % 8;
        }
        int offset = WhiteToMove ? 0 : 56;
        switch (move.StartIndex + offset, WhiteToMove)
        { // moving a rook disables castling.
            case (0, true):
                WhiteCanCastleQueen = false;
                break;
            case (7, true):
                WhiteCanCastleKing = false;
                break;
            case (56, false):
                BlackCanCastleQueen = false;
                break;
            case (63, false):
                BlackCanCastleKing = false;
                break;
        }
        return fmc;
    }
    private void RemovePiece(int piece, int square)
    {
        int colour = Piece.Colour(piece);
        int index = colour == 8 ? 0 : 1;
        int pieceType = piece - colour;
        switch (pieceType)
        {
            case 2:
                Pawns[index].RemovePieceAtSquare(square);
                break;
            case 3:
                Knights[index].RemovePieceAtSquare(square);
                break;
            case 5:
                Bishops[index].RemovePieceAtSquare(square);
                break;
            case 6:
                Rooks[index].RemovePieceAtSquare(square);
                break;
            case 7:
                Queens[index].RemovePieceAtSquare(square);
                break;
        }
        Squares[square] = 0;
    }
    private void MovePiece(int piece, Move move)
    {
        int colour = Piece.Colour(piece);
        int index = colour == 8 ? 0 : 1;
        int pieceType = piece - colour;
        switch (pieceType)
        {
            case 1:
                KingSquares[index] = move.EndIndex;
                break;
            case 2:
                Pawns[index].MovePiece(move.StartIndex, move.EndIndex);
                break;
            case 3:
                Knights[index].MovePiece(move.StartIndex, move.EndIndex);
                break;
            case 5:
                Bishops[index].MovePiece(move.StartIndex, move.EndIndex);
                break;
            case 6:
                Rooks[index].MovePiece(move.StartIndex, move.EndIndex);
                break;
            case 7:
                Queens[index].MovePiece(move.StartIndex, move.EndIndex);
                break;
        }
        Squares[move.StartIndex] = 0;
        Squares[move.EndIndex] = piece;
    }
}
