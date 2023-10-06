﻿using System.Reflection;

namespace ChessLogic;

public class MoveGenerator
{
    private Board board;
    private bool whiteToMove;
    private bool inCheck;
    private bool inDoubleCheck;
    private int inKnightCheck = -1;
    public MoveGenerator(Board board)
    {
        this.board = board;
        whiteToMove = board.WhiteToMove;
    }
    private void Init()
    {
        inCheck = false;
        inDoubleCheck = false;
        inKnightCheck = -1;
    }
    public List<Move> GenerateMoves()
    {
        Init();
        (var attackMap, var pins) = CreateAttackMap();
        List<Move> moves = GenerateKingMoves(attackMap);
        if (inDoubleCheck)
        {
            return moves;
        }
        if (inKnightCheck != -1)
        {  // can either move or take the attacking knight
            moves.AddRange(TakeAttackingKnight(pins));
        }
        return moves;
    }

    private List<Move> TakeAttackingKnight(List<Pin> pins)
    {
        int friendlyPieceInd = whiteToMove ? 0 : 1;
        List<Move> moves = new();
        for (int i = 0; i < board.Bishops[friendlyPieceInd].Count; i++)
        {
            int bishop = board.Bishops[friendlyPieceInd][i];
            if (CanTakeKnightSlidingPiece(bishop, 4, 8, pins))
            {
                moves.Add(new(bishop, inKnightCheck));
            }
        }
        for (int i = 0; i < board.Rooks[friendlyPieceInd].Count; i++)
        {
            int rook = board.Rooks[friendlyPieceInd][i];
            if (CanTakeKnightSlidingPiece(rook, 0, 4, pins))
            {
                moves.Add(new(rook, inKnightCheck));
            }
        }
        for (int i = 0; i < board.Queens[friendlyPieceInd].Count; i++)
        {
            int queen = board.Queens[friendlyPieceInd][i];
            if (CanTakeKnightSlidingPiece(queen, 4, 8, pins))
            {
                moves.Add(new(queen, inKnightCheck));
            }
        }
        moves.AddRange(CanTakeKnightPawns(pins));
        moves.AddRange(CanTakeKnightKnight(pins));
        return moves;
    }

    private IEnumerable<Move> CanTakeKnightKnight(List<Pin> pins)
    {
        int ind = whiteToMove ? 0 : 1;
        List<Move> moves = new();
        for (int i = 0; i < board.Knights[ind].Count; i++)
        {
            int knight = board.Knights[ind][i];
            if (pins.Where(x => x.Square == knight).Count() != 0)
            {
                continue;
            }
            if (Board.KnightMoves.Contains(inKnightCheck - knight))
            {
                moves.Add(new(knight, inKnightCheck));
            }
        }
        return moves;
    }

    private List<Move> CanTakeKnightPawns(List<Pin> pins)
    {
        int ind = whiteToMove ? 0 : 1;
        int coef = whiteToMove ? 1 : -1;
        int leftAttack = 7 * coef, rightAttack = 9 * coef;
        List<Move> moves = new();
        for (int i = 0; i < board.Pawns[ind].Count; i++)
        {
            int pawn = board.Pawns[ind][i];
            if (pins.Where(x => x.Square == pawn).Count() != 0)
            {  // there is no way pawn could move and take knight if it is smh pinned
                continue;
            }
            if (Math.Abs(pawn % 8 - (pawn + leftAttack) % 8) == 1 && pawn + leftAttack == inKnightCheck)
            {
                moves.Add(new Move(pawn, inKnightCheck));
            }
            if (Math.Abs(pawn % 8 - (pawn + rightAttack) % 8) == 1 && pawn + rightAttack == inKnightCheck)
            {
                moves.Add(new Move(pawn, inKnightCheck));
            }
        }
        return moves;
    }

    private bool CanTakeKnightSlidingPiece(int piece, int startIndex, int endIndex, List<Pin> pins)
    {
        int difference =  inKnightCheck - piece;
        if (pins.Where(x => x.Square == piece).Count() == 2)
        {
            return false;
        }
        int movingDir = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            int dir = Board.SlidingDirections[i];
            if (difference > 8 && Math.Abs(dir) == 1)
            {
                continue;
            }
            if (difference % dir == 0 && difference * dir > 0)
            {
                movingDir = dir;
                break;
            }
        }
        var pin = pins.Where(x => x.Square == piece).FirstOrDefault();
        if (movingDir == 0 || (pin != null && pin.Direction != movingDir))
        {
            return false;
        }
        piece += movingDir;
        while (0 <= piece && piece < 64)
        {
            if (piece == inKnightCheck)
            {
                return true;
            }
            if (Piece.None != board.Squares[piece])
            {
                return false;
            }
            piece += movingDir;
        }
        return false;
    }

    private List<Move> GenerateKingMoves(HashSet<int> attackMap)
    {
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        int friendlyColour = whiteToMove ? 8 : 16;
        var moves = new List<Move>();
        foreach (var direction in Board.SlidingDirections)
        {
            int move = kingSquare + direction;
            if (Math.Abs(kingSquare % 8 - (move) % 8) > 1 || move < 0 
                || move > 63 || attackMap.Contains(move) || Piece.IsColour(board.Squares[move], friendlyColour))
            {
                continue;
            }
            moves.Add(new(kingSquare, move));
        }
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
        IndirectAttacksMap(attackMap, attackingPiecesInd);
        PawnAttacks(attackMap, kingSquare, attackingPiecesInd);  // i can probably put in SideAttacksMap but i don't feel like it
        return (attackMap, pins);
    }

    private void PawnAttacks(HashSet<int> attackMap, int kingSquare, int attackingPieceInd)
    {
        int coef = whiteToMove ? -1 : 1;
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
            int square = kingSquare + direction;
            attackedSquares.Add(kingSquare);
            int undefendedAttacker = -1;
            bool foundAttacker = false;
            int friendlyPiece = -1;
            while (0 <= square && square < 64)
            {
                int piece = board.Squares[square];
                if (piece == Piece.None)
                {
                    attackedSquares.Add(square);
                    if (Math.Abs(square % 8 - (square + direction) % 8) > 1)
                    {
                        break;
                    }
                    square += direction;
                    continue;
                }
                if (!Piece.IsColour(piece, attackingColour) && !foundAttacker)
                {  // case 1
                    friendlyPiece = square;
                    attackedSquares.Clear();
                    attackedSquares.Add(square);
                }
                if (!Piece.IsColour(piece, attackingColour) && foundAttacker)
                {  // case 4
                    friendlyPiece = friendlyPiece == -1 ? square : friendlyPiece;
                    attackedSquares.Add(square);
                    break;
                }
                if (checkFunc(piece) && Piece.IsColour(piece, attackingColour))
                {  // case 2 + 5
                    attackedSquares.Add(undefendedAttacker);  // kinda complicated but has a point
                    undefendedAttacker = foundAttacker ? square : undefendedAttacker;
                    attackedSquares.Add(undefendedAttacker);
                    foundAttacker = true;
                    undefendedAttacker = square;
                }
                if (Piece.IsColour(piece, attackingColour) && !foundAttacker)
                {  // case 3
                    attackedSquares.Clear();
                    attackedSquares.Add(square);
                }
                if (Piece.IsColour(piece, attackingColour) && foundAttacker && !checkFunc(piece))
                {
                    attackedSquares.Add(square);
                    break;
                }
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
            if (foundAttacker && attackedSquares.Contains(kingSquare))
            {
                inDoubleCheck = inCheck ? true : false;
                inCheck = true;
            }
            if (foundAttacker)
            {
                attackMap.UnionWith(attackedSquares);
            }
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
                    inKnightCheck = knight;
                }
                attackMap.Add(attackedSquare);
            }
        }
    }
    private void IndirectAttacksMap(HashSet<int> attackMap, int attackingPieceInd)
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
        var enemyKing = whiteToMove ? board.KingSquares[1] : board.KingSquares[0];
        for (int i = 0; i < 8; i++)
        {
            int dir = Board.SlidingDirections[i];
            int square = enemyKing + dir;
            if (Math.Abs(enemyKing % 8 - (square) % 8) > 1 || square < 0 || square > 63)
            {
                continue;
            }
            attackMap.Add(square);
        }
    }
    private void ExtendAttackMap(HashSet<int> attackMap, int attackingPieceInd, int piece,
        int startIndex, int endIndex)
    {
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        for (int j = startIndex; j < endIndex; j++)
        {
            int dir = Board.SlidingDirections[j];
            int square = piece + dir;
            while (0 <= square && square < 64)
            {
                attackMap.Add(square);
                if (Piece.None != board.Squares[square] && square != kingSquare)
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
