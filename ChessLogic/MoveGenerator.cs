using System.Reflection;

namespace ChessLogic;

public class MoveGenerator
{
    private Board board;
    private bool whiteToMove;
    private bool inCheck;
    private bool inDoubleCheck;
    private bool inKnightCheck;
    public MoveGenerator(Board board)
    {
        this.board = board;
        whiteToMove = board.WhiteToMove;
    }
    public List<Move> GenerateMoves()
    {
        inCheck = false;
        inDoubleCheck = false;
        (var attackMask, var pins) = CreateAttackMap();

        List<Move> moves = new();
        return moves;
    }

    private (HashSet<int>, List<Pin>) CreateAttackMap()
    {
        HashSet<int> attackMap = new();
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        int attackingPiecesInd = whiteToMove ? 1 : 0;
        int attackingColour = whiteToMove ? 16 : 8;
        int startIndex = 0, endIndex = 8;
        List<Pin> pins = new(1);
        if (board.Bishops[attackingPiecesInd].Count > 0 || board.Queens[attackingPiecesInd].Count > 0)
        {
            // some pieces can attack diagonaly
            pins.AddRange(ExtendAttackMapSlidingPieces(attackMap, kingSquare, attackingColour, Piece.IsBishopOrQueen, 4, 8));
        }
        if (board.Rooks[attackingPiecesInd].Count > 0 || board.Queens[attackingPiecesInd].Count > 0)
        {
            // some pieces can attack in straight line
            pins.AddRange(ExtendAttackMapSlidingPieces(attackMap, kingSquare, attackingColour, Piece.IsRookOrQueen, 0, 4));
        }
        if (board.Knights[attackingPiecesInd].Count > 0)
        {  // knight attacks
            ExtendAttackMapKnights(attackMap, kingSquare, board.Knights[attackingPiecesInd]);
        }
        SideAttacksMap(attackMap, attackingPiecesInd);
        PawnAttacks(attackMap, kingSquare, attackingPiecesInd);  // i can probably put in SideAttacksMap but i don't feel like it
        return (attackMap, pins);
    }

    private void PawnAttacks(HashSet<int> attackMap, int kingSquare, int attackingPieceInd)
    {
        int coef = whiteToMove ? 1 : -1;
        int leftAttack = 7 * coef, rightAttack = 9 * coef;
        for (int i = 0; i < board.Pawns[attackingPieceInd].Count; i++)
        {
            int piece = board.Pawns[attackingPieceInd][i];
            if (Math.Abs(piece % 8 - (piece + leftAttack) % 8) == 1)
            {
                attackMap.Add(piece + leftAttack);
            }
            if (Math.Abs(piece % 8 - (piece + rightAttack) % 8) == 1)
            {
                attackMap.Add(piece + rightAttack);
            }
        }
    }

    private List<Pin> ExtendAttackMapSlidingPieces(HashSet<int> attackMap, int kingSquare, int attackingColour,
        Func<int, bool> checkFunc, int startIndex, int endIndex)
    {
        List<Pin> pins = new(1);  // generaly speaking, 1 pins is expected in a position
        HashSet<int> attackedSquares = new();
        for (int i = startIndex; i < endIndex; i++)
        {
            int direction = Board.SlidingDirections[i];
            int square = kingSquare;
            attackedSquares.Add(kingSquare);
            int undefendedAttacker = -1;
            bool foundAttacker = false;
            int friendlyPiece = -1;
            while (0 <= square && square < 64)
            {
                int piece = board.Squares[square];
                if (!Piece.IsColour(piece, attackingColour) && !foundAttacker)
                {  // case 1
                    friendlyPiece = square;
                    attackedSquares.Clear();
                }
                if (!Piece.IsColour(piece, attackingColour) && foundAttacker)
                {  // case 4
                    friendlyPiece = square;
                    attackedSquares.Add(square);
                    break;
                }
                if (checkFunc(piece) && Piece.IsColour(piece, attackingColour))
                {  // case 2 + 5
                    undefendedAttacker = foundAttacker ? square : undefendedAttacker;
                    foundAttacker = true;
                    attackedSquares.Add(undefendedAttacker == -1 ? square : undefendedAttacker);
                    undefendedAttacker = square;
                }
                else if (Piece.IsColour(piece, attackingColour) && !foundAttacker)
                {  // case 3
                    attackedSquares.Clear();
                    attackedSquares.Add(square);
                }
                attackedSquares.Add(square);
                if (Math.Abs(square % 8 - (square + direction) % 8) > 1)
                {
                    break;
                }
                square += direction;
            }
            if (foundAttacker && friendlyPiece != -1)
            {
                pins.Add(new Pin(friendlyPiece, direction));
            }
            if (foundAttacker && friendlyPiece == -1)
            {
                inDoubleCheck = inCheck ? true : false;
                inCheck = true;
            }
            attackMap.UnionWith(attackedSquares);
            attackedSquares.Clear();
        }
        return pins;
    }
    // We check all opponent Knights and extend attack map, also look for checks
    private void ExtendAttackMapKnights(HashSet<int> attackMap, int kingSquare, PieceList knights)
    {
        for(int i = 0; i < knights.Count; i++)
        {
            int knight = knights[i];
            foreach (var direction in Board.KnightMoves)
            {
                int attackedSquare = knight + direction;
                if (0 > attackedSquare || attackedSquare >= 64)
                {
                    continue;
                }
                if (attackedSquare == kingSquare)
                {
                    inDoubleCheck = inCheck ? true : false;
                    inKnightCheck = true;
                }
                attackMap.Add(attackedSquare);
            }
        }
    }
    private void SideAttacksMap(HashSet<int> attackMap, int attackingPieceInd)
    {
        for (int i = 0; i < board.Bishops[attackingPieceInd].Count; i++)
        {
            ExtendAttackMap(attackMap, attackingPieceInd, board.Bishops[attackingPieceInd][i], 4, 8);
        }
        for (int i = 0; i < board.Rooks[attackingPieceInd].Count; i++)
        {
            ExtendAttackMap(attackMap, attackingPieceInd, board.Rooks[attackingPieceInd][i], 0, 4);
        }
        for (int i = 0; i < board.Queens[attackingPieceInd].Count; i++)
        {
            ExtendAttackMap(attackMap, attackingPieceInd, board.Queens[attackingPieceInd][i], 0, 8);
        }
    }
    private void ExtendAttackMap(HashSet<int> attackMap, int attackingPieceInd, int piece,
        int startIndex, int endIndex)
    {
        for (int j = startIndex; j < endIndex; j++)
        {
            int dir = Board.SlidingDirections[j];
            int square = piece + dir;
            while (0 <= square && square < 64)
            {
                attackMap.Add(square);
                if (Piece.None != piece)
                {
                    break;
                }
                if (Math.Abs(square % 8 - (square + dir) % 8) > 1)
                {
                    break;
                }
                square += dir;
            }
        }
    }
}
