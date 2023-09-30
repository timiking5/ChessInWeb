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
    public int FiftyMoveCounter { get; set; } = 0;
    public int PlysCounter { get; set; } = 0;
    public int[] KingSquares { get; set; }
    public bool WhiteCanCastleKing { get; set; }
    public bool WhiteCanCastleQueen { get; set; }
    public bool BlackCanCastleKing { get; set; }
    public bool BlackCanCastleQueen { get; set; }
    public int colourToMoveIndex { get; set; }
    public PieceList[] Rooks { get; set; }
    public PieceList[] Bishops { get; set; }
    public PieceList[] Queens { get; set; }
    public PieceList[] Knights { get; set; }
    public PieceList[] Pawns { get; set; }
    private int EnPassantCol = -1;  // tells if there is an opportunity to En Passant
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
        bool enPassant = int.TryParse(meta[3], out int ePCol);
        EnPassantCol = enPassant ? ePCol : -1;
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
}
