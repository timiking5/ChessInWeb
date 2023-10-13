using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Transactions;
using System.Xml;

namespace ChessLogic;

public class MoveGenerator
{
    private Board board;
    private bool whiteToMove;
    private int inCheck;
    private bool inDoubleCheck;
    private int inKnightCheck = -1;
    private List<Pin> pins = new(1);
    public MoveGenerator(Board board)
    {
        this.board = board;
        whiteToMove = board.WhiteToMove;
    }
    private void Init()
    {
        pins.Clear();
        inCheck = -1;
        inDoubleCheck = false;
        inKnightCheck = -1;
    }
    public List<Move> GenerateMoves()
    {
        Init();
        var attackMap = CreateAttackMap();
        List<Move> moves = GenerateKingMoves(attackMap);
        if (inDoubleCheck)
        {
            return moves;
        }
        if (inKnightCheck != -1 || inCheck != -1)
        {  // can either move king or take the attacking piece
            moves.AddRange(TakeAttackingPiece(inKnightCheck));
        }
        if (inCheck != -1)
        { // If it is not a knight check, then we can cover king from check
            moves.AddRange(CoverKing());
        }
        if (inCheck == -1 && inKnightCheck == -1)
        {
            moves.AddRange(GeneratePiecesMoves(attackMap));
        }
        return moves;
    }

    private List<Move> GeneratePiecesMoves(HashSet<int> attackMap)
    {
        int index = whiteToMove ? 0 : 1;
        List<Move> moves = new();
        for (int i = 0; i < board.Bishops[index].Count; i++)
        {
            moves.AddRange(GenerateSlidingPiecesMoves(board.Bishops[index][i], 4, 8));
        }
        for (int i = 0; i < board.Rooks[index].Count; i++)
        {
            moves.AddRange(GenerateSlidingPiecesMoves(board.Rooks[index][i], 0, 4));
        }
        for (int i = 0; i < board.Queens[index].Count; i++)
        {
            moves.AddRange(GenerateSlidingPiecesMoves(board.Queens[index][i], 0, 8));
        }
        moves.AddRange(GenerateKnightMoves());
        moves.AddRange(GenerateCastling(attackMap));
        moves.AddRange(GeneratePawnMoves());
        // TODO - Generate castling and pawns moves;
        return moves;
    }

    private List<Move> GeneratePawnMoves()
    {
        List<Move> moves = new();
        int index = whiteToMove ? 0 : 1;
        int coef = whiteToMove ? 1 : -1;
        int leftAttack = 7 * coef, rightAttack = 9 * coef;
        int startingRow = whiteToMove ? 1 : 6, endRow = whiteToMove ? 6 : 1;
        for (int i = 0; i < board.Pawns[index].Count; i++)
        {
            int pawn = board.Pawns[index][i];
            moves.AddRange(GenerateMovesForPawn(pawn, leftAttack, rightAttack, startingRow, endRow, coef));
        }
        if (board.EnPassantCol != -1)
        {
            moves.AddRange(GenerateEnPassant());
        }

        return moves;
    }

    private List<Move> GenerateEnPassant()
    {
        List<Move> moves = new();
        int enPassantRow = whiteToMove ? 4 : 3;
        int leftIndex = enPassantRow * 8 + board.EnPassantCol - 1;
        int rightIndex = enPassantRow * 8 + board.EnPassantCol + 1;
        int leftPiece = board.Squares[leftIndex];
        int rightPiece = board.Squares[rightIndex];
        int enPassantPawn = board.Squares[enPassantRow * 8 + board.EnPassantCol];
        int pawn = -1;
        if (CanDoEnpassant(leftIndex, leftPiece, enPassantPawn, 9) && CanDoEnpassant(rightIndex, rightPiece, enPassantPawn, 7))
        {
            moves.Add(new(leftIndex, (enPassantRow + 1) * 8 + board.EnPassantCol, 1));
            moves.Add(new(rightIndex, (enPassantRow + 1) * 8 + board.EnPassantCol, 1));
            return moves;
        }
        if (CanDoEnpassant(leftIndex, leftPiece, enPassantPawn, 9))
        {
            pawn = leftIndex;
        }
        if (CanDoEnpassant(rightIndex, rightPiece, enPassantPawn, 7))
        {
            pawn = rightIndex;
        }
        if (pawn == -1)
        {
            return moves;
        }
        if (EnPassantLegal(pawn))
        {
            moves.Add(new(pawn, (enPassantRow + 1) * 8 + board.EnPassantCol, 1));
        }

        return moves;
    }

    private bool EnPassantLegal(int pawn)
    {
        bool foundKing = false;
        int attacker = -1;
        int friendlyColor = whiteToMove ? 8 : 16;
        int attackingColour = whiteToMove ? 16 : 8;
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        int start = pawn / 8 * 8, end = pawn / 8 * 8 + 8;
        int square = start;
        if (kingSquare / 8 != pawn / 8)
        {
            return true;
        }
        while (square < end)
        {
            if (board.Squares[square] == Piece.None || Piece.IsPawn(board.Squares[square]))
            {
                square += 1;
                continue;
            }
            if (square == kingSquare)
            {
                foundKing = true;
                square += 1;
                continue;
            }
            if (foundKing && attacker == -1 && Piece.IsColour(board.Squares[square], friendlyColor))
            {
                return true;
            }
            if (foundKing && attacker == -1 && Piece.IsColour(board.Squares[square], attackingColour))
            {
                attacker = square;
            }
            if (!foundKing && Piece.IsColour(board.Squares[square], attackingColour))
            {
                attacker = square;
            }
            square += 1;
        }
        if (attacker != -1 && Piece.IsRookOrQueen(board.Squares[attacker]))
        { // king will always be found by now
            return false;
        }
        return true;
    }

    private bool CanDoEnpassant(int square, int piece, int enPassantPawn, int captDir)
    {
        var pawnPins = pins.Where(x => x.Square == square);
        if (pawnPins.Count() == 2)
        {
            return false;
        }
        bool pinned = false;
        int pinDir = 0;
        if (pawnPins.Count() == 1)
        {
            pinned = true;
            pinDir = pawnPins.First().Direction;
        }
        return Piece.IsPawn(piece) && !Piece.IsColour(piece, Piece.Colour(enPassantPawn)) && (!pinned || Math.Abs(pinDir) == Math.Abs(captDir));
    }

    private List<Move> GenerateMovesForPawn(int pawn, int leftAttack, int rightAttack, int startingRow, int endRow, int coef)
    {  // PINS!!!
        List<Move> moves = new();
        bool pinned = false;
        int dir = 0;
        var pawnPins = pins.Where(x => x.Square == pawn);
        if (pawnPins.Count() == 2)
        {
            return moves;
        }
        if (pawnPins.Count() == 1)
        {
            pinned = true;
            dir = pawnPins.First().Direction;
        }
        if (pawn / 8 == startingRow && InBounds(pawn + 16 * coef)
            && board.Squares[pawn + 8 * coef] == Piece.None && board.Squares[pawn + 16 * coef] == Piece.None
            && PinnedCanMove(pinned, 8, dir))
        {
            moves.Add(new(pawn, pawn + 16 * coef, 7));
        }
        if (InBounds(pawn + 8 * coef) && board.Squares[pawn + 8 * coef] == Piece.None && PinnedCanMove(pinned, 8, dir))
        {
            if (pawn / 8 != endRow)
            {
                moves.Add(new(pawn, pawn + rightAttack));
            }
            else
            {
                moves.AddRange(new List<Move>()
                {
                    new(pawn, pawn + 8 * coef, 3),
                    new(pawn, pawn + 8 * coef, 4),
                    new(pawn, pawn + 8 * coef, 5),
                    new(pawn, pawn + 8 * coef, 6),
                });
            }
        }
        if (InBounds(pawn + leftAttack) && board.Squares[pawn + leftAttack] != Piece.None
            && !Piece.IsColour(board.Squares[pawn + leftAttack], board.Squares[pawn])
            && PinnedCanMove(pinned, leftAttack, dir))
        {
            if (pawn / 8 != endRow)
            {
                moves.Add(new(pawn, pawn + leftAttack));
            }
            else
            {
                moves.AddRange(new List<Move>()
                {
                    new(pawn, pawn + leftAttack, 3),
                    new(pawn, pawn + leftAttack, 4),
                    new(pawn, pawn + leftAttack, 5),
                    new(pawn, pawn + leftAttack, 6),
                });
            }
        }
        if (InBounds(pawn + rightAttack) && board.Squares[pawn + rightAttack] != Piece.None
            && !Piece.IsColour(board.Squares[pawn + rightAttack], board.Squares[pawn])
            && PinnedCanMove(pinned, rightAttack, dir))
        {
            if (pawn / 8 != endRow)
            {
                moves.Add(new(pawn, pawn + rightAttack));
            }
            else
            {
                moves.AddRange(new List<Move>()
                {
                    new(pawn, pawn + rightAttack, 3),
                    new(pawn, pawn + rightAttack, 4),
                    new(pawn, pawn + rightAttack, 5),
                    new(pawn, pawn + rightAttack, 6),
                });
            }
        }
        return moves;
    }
    private bool PinnedCanMove(bool pinned, int moveDir, int pinDir)
    {
        return !pinned || Math.Abs(moveDir) == Math.Abs(pinDir);
    }
    private static int[] shortCastling = new[] { 5, 6 };
    private static int[] longCastling = new[] { 1, 2, 3 };
    private static int kingSquare = 4;
    private static int shortCastleSquare = 6;
    private static int longCastleSquare = 2;
    private List<Move> GenerateCastling(HashSet<int> attackMap)
    {
        bool canGoShort = whiteToMove ? board.WhiteCanCastleKing : board.BlackCanCastleKing;
        bool canGoLong = whiteToMove ? board.WhiteCanCastleQueen : board.BlackCanCastleQueen;
        int offset = whiteToMove ? 0 : 56;
        List<Move> moves = new();
        if (canGoShort && CanCastle(shortCastling, attackMap))
        {
            moves.Add(new(kingSquare + offset, shortCastleSquare + offset, 2));
        }
        if (canGoLong && CanCastle(longCastling, attackMap))
        {
            moves.Add(new(kingSquare + offset, longCastleSquare + offset, 2));
        }
        return moves;
    }

    private bool CanCastle(int[] squaresToCheck, HashSet<int> attackMap)
    {
        int offset = whiteToMove ? 0 : 56;
        for (int i = 0; i < squaresToCheck.Length; i++)
        {
            int square = squaresToCheck[i] + offset;
            if (board.Squares[square] != Piece.None || attackMap.Contains(square))
            {
                return false;
            }
        }
        return true;
    }

    private List<Move> GenerateKnightMoves()
    {
        int index = whiteToMove ? 0 : 1;
        int colour = whiteToMove ? 8 : 16;
        List<Move> moves = new();
        for (int i = 0; i < board.Knights[index].Count; i++)
        {
            int knight = board.Knights[index][i];
            if (pins.Where(x => x.Square == knight).Any())
            {
                continue;
            }
            foreach (var dir in Board.KnightMoves)
            {
                int attackedSquare = knight + dir;
                if (0 > attackedSquare || attackedSquare >= 64 || Math.Abs(knight % 8 - (knight + dir) % 8) > 2
                    || Piece.IsColour(board.Squares[attackedSquare], colour))
                {
                    continue;
                }
                moves.Add(new(knight, attackedSquare));
            }
        }
        return moves;
    }

    private List<Move> GenerateSlidingPiecesMoves(int piece, int startIndex, int endIndex)
    {
        List<Move> moves = new();
        int colour = Piece.Colour(board.Squares[piece]);
        var piecePins = pins.Where(x => x.Square == piece);
        if (pins.Count > 1)
        {
            return moves;
        }
        bool pinned = false;
        int pinnedDir = -1000;
        if (pins.Count == 1)
        {
            pinned = true;
            pinnedDir = pins.First().Direction;
        }
        for (int i = startIndex; i < endIndex; i++)
        {
            int dir = Board.SlidingDirections[i];
            dir = !pinned || (pinned && pinnedDir == dir) ? dir : -1000;
            int square = piece;
            while (0 <= square + dir && square + dir < 64)
            {
                if (Math.Abs(square % 8 - (square + dir) % 8) > 1)
                {
                    break;
                }
                square += dir;
                if (board.Squares[square] == Piece.None)
                {
                    moves.Add(new(piece, square));
                    continue;
                }
                if (Piece.IsColour(board.Squares[square], colour))
                {
                    break;
                }
                moves.Add(new(piece, square));
                break;
            }
        }
        return moves;
    }

    private List<Move> CoverKing()
    {
        List<Move> moves = new();
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        int difference = inCheck - kingSquare;
        int attackingDir = -100;
        // Generate all squares where a piece can block the attack
        var squaresToBlock = SquaresToBlock(kingSquare, difference, attackingDir);
        int index = whiteToMove ? 0 : 1;
        for (int i = 0; i < board.Bishops[index].Count; i++)
        {
            int bishop = board.Bishops[index][i];
            if (pins.Where(x => x.Square == bishop).Any())  // CONSIDER CHANGING COUNT 1
            {  // piece can't block
                continue;
            }
            moves.AddRange(BlockWithSlidingPiece(bishop, squaresToBlock, 4, 8));
        }
        for (int i = 0; i < board.Rooks[index].Count; i++)
        {
            int rook = board.Rooks[index][i];
            if (pins.Where(x => x.Square == rook).Any())  // CONSIDER CHANGING COUNT 1
            {  // piece can't block
                continue;
            }
            moves.AddRange(BlockWithSlidingPiece(rook, squaresToBlock, 0, 4));
        }
        for (int i = 0; i < board.Queens[index].Count; i++)
        {
            int queen = board.Queens[index][i];
            if (pins.Where(x => x.Square == queen).Any())  // CONSIDER CHANGING COUNT 1
            {  // piece can't block
                continue;
            }
            moves.AddRange(BlockWithSlidingPiece(queen, squaresToBlock, 4, 8));
        }
        moves.AddRange(BlockWithKnight(squaresToBlock));
        moves.AddRange(BlockWithPawns(squaresToBlock));
        return moves;
    }

    private List<Move> BlockWithPawns(HashSet<int> squaresToBlock)
    {
        int coef = whiteToMove ? 1 : -1;
        int index = whiteToMove ? 0 : 1;
        List<Move> moves = new();
        for (int i = 0; i < board.Pawns[index].Count; i++)
        {
            int pawn = board.Pawns[index][i];
            var pawnPins = pins.Where(x => x.Square == pawn);
            if (pawnPins.Count() > 1)
            {
                continue;
            }
            if (pawnPins.Count() == 1 && Math.Abs(pins.First().Direction) != 8)
            {
                continue;
            }
            if (squaresToBlock.Contains(pawn + 8 * coef))
            {
                moves.Add(new(pawn, pawn + 8 * coef));
            }
            if (squaresToBlock.Contains(pawn + 16 * coef))
            {
                moves.Add(new(pawn, pawn + 16 * coef));
            }
        }

        return moves;
    }

    private List<Move> BlockWithKnight(HashSet<int> squaresToBlock)
    {
        List<Move> moves = new();
        int index = whiteToMove ? 0 : 1;
        for (int i = 0; i < board.Knights[index].Count; i++)
        {
            int knight = board.Knights[index][i];
            if (pins.Where(x => x.Square == knight).Any())
            {
                continue;
            }
            foreach (var block in squaresToBlock)
            {
                if (Board.KnightMoves.Contains(knight - block))
                {
                    moves.Add(new(knight, inCheck));
                }
            }
        }
        return moves;
    }

    private List<Move> BlockWithSlidingPiece(int piece, HashSet<int> squaresToBlock, int startIndex, int endIndex)
    {
        HashSet<int> directions = new();
        List<Move> moves = new();
        for (int i = startIndex; i < endIndex; i++)
        {
            int dir = Board.SlidingDirections[i];
            foreach (var blocks in squaresToBlock)
            {
                int difference = blocks - piece;
                if (difference % dir == 0 && difference * dir > 0)
                {
                    directions.Add(dir);
                }
            }
        }
        foreach (var dir in directions)
        {
            int square = dir + piece;
            while (0 <= square && square < 64)
            {
                if (squaresToBlock.Contains(square))
                {
                    moves.Add(new(piece, square));
                }
                if (board.Squares[square] != Piece.None || Math.Abs(square % 8 - (square + dir) % 8) > 1)
                {
                    break;
                }
                square += dir;
            }
        }
        return moves;
    }

    private HashSet<int> SquaresToBlock(int kingSquare, int difference, int attackingDir)
    {
        foreach (var direction in Board.SlidingDirections)
        {
            if (Math.Abs(direction) == 1 && difference > 8)
            {
                continue;
            }
            if (difference % direction == 0 && direction * difference > 0)
            {
                attackingDir = direction;
                break;
            }
        }
        HashSet<int> squaresToBlock = new();
        int square = kingSquare + attackingDir;
        while (square != inCheck)
        {
            squaresToBlock.Add(square);
            square += attackingDir;
        }

        return squaresToBlock;
    }

    private List<Move> TakeAttackingPiece(int attackingPiece)
    {
        int friendlyPieceInd = whiteToMove ? 0 : 1;
        List<Move> moves = new();
        for (int i = 0; i < board.Bishops[friendlyPieceInd].Count; i++)
        {
            int bishop = board.Bishops[friendlyPieceInd][i];
            if (CanTakeAttackerSlidingPiece(bishop, 4, 8, attackingPiece))
            {
                moves.Add(new(bishop, attackingPiece));
            }
        }
        for (int i = 0; i < board.Rooks[friendlyPieceInd].Count; i++)
        {
            int rook = board.Rooks[friendlyPieceInd][i];
            if (CanTakeAttackerSlidingPiece(rook, 0, 4, attackingPiece))
            {
                moves.Add(new(rook, attackingPiece));
            }
        }
        for (int i = 0; i < board.Queens[friendlyPieceInd].Count; i++)
        {
            int queen = board.Queens[friendlyPieceInd][i];
            if (CanTakeAttackerSlidingPiece(queen, 4, 8, attackingPiece))
            {
                moves.Add(new(queen, attackingPiece));
            }
        }
        moves.AddRange(CanTakeAttackerPawns(attackingPiece));
        moves.AddRange(CanTakeAttackerKnight(attackingPiece));
        return moves;
    }

    private List<Move> CanTakeAttackerKnight(int attackingPiece)
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
            if (Board.KnightMoves.Contains(attackingPiece - knight))
            {
                moves.Add(new(knight, attackingPiece));
            }
        }
        return moves;
    }

    private List<Move> CanTakeAttackerPawns(int attackingPiece)
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
            if (Math.Abs(pawn % 8 - (pawn + leftAttack) % 8) == 1 && pawn + leftAttack == attackingPiece)
            {
                moves.Add(new Move(pawn, attackingPiece));
            }
            if (Math.Abs(pawn % 8 - (pawn + rightAttack) % 8) == 1 && pawn + rightAttack == attackingPiece)
            {
                moves.Add(new Move(pawn, attackingPiece));
            }
        }
        return moves;
    }

    private bool CanTakeAttackerSlidingPiece(int piece, int startIndex, int endIndex, int attackingPiece)
    {
        int difference = attackingPiece - piece;
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
        if (movingDir == 0 || (pin != null && Math.Abs(pin.Direction) != Math.Abs(movingDir)))
        {
            return false;
        }
        piece += movingDir;
        while (0 <= piece && piece < 64)
        {
            if (piece == attackingPiece)
            {
                return true;
            }
            if (Piece.None != board.Squares[piece])
            {
                return false;
            }
            if (Math.Abs(piece % 8 - (piece + movingDir) % 8) > 1)
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

    private HashSet<int> CreateAttackMap()
    {
        HashSet<int> attackMap = new();
        int kingSquare = whiteToMove ? board.KingSquares[0] : board.KingSquares[1];
        int attackingPiecesInd = whiteToMove ? 1 : 0;
        int attackingColour = whiteToMove ? 16 : 8;
        int startIndex = 0, endIndex = 8;
        if (board.Bishops[attackingPiecesInd].Count > 0 || board.Queens[attackingPiecesInd].Count > 0)
        {
            // some pieces can attack diagonaly
            ExtendAttackMapSlidingPieces(attackMap, kingSquare, attackingColour, Piece.IsBishopOrQueen, 4, 8);
        }
        if (board.Rooks[attackingPiecesInd].Count > 0 || board.Queens[attackingPiecesInd].Count > 0)
        {
            // some pieces can attack in straight line
            ExtendAttackMapSlidingPieces(attackMap, kingSquare, attackingColour, Piece.IsRookOrQueen, 0, 4);
        }
        if (board.Knights[attackingPiecesInd].Count > 0)
        {  // knight attacks
            ExtendAttackMapKnights(attackMap, kingSquare, board.Knights[attackingPiecesInd]);
        }
        IndirectAttacksMap(attackMap, attackingPiecesInd);
        PawnAttacks(attackMap, kingSquare, attackingPiecesInd);  // i can probably put in SideAttacksMap but i don't feel like it
        return attackMap;
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

    private void ExtendAttackMapSlidingPieces(HashSet<int> attackMap, int kingSquare, int attackingColour,
        Func<int, bool> checkFunc, int startIndex, int endIndex)
    {
        HashSet<int> attackedSquares = new();
        for (int i = startIndex; i < endIndex; i++)
        {
            int direction = Board.SlidingDirections[i];
            int square = kingSquare + direction;
            attackedSquares.Add(kingSquare);
            int undefendedAttacker = -1;
            int foundAttacker = -1;
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
                if (!Piece.IsColour(piece, attackingColour) && foundAttacker == -1)
                {  // case 1
                    friendlyPiece = square;
                    attackedSquares.Clear();
                    attackedSquares.Add(square);
                }
                if (!Piece.IsColour(piece, attackingColour) && foundAttacker != -1)
                {  // case 4
                    friendlyPiece = friendlyPiece == -1 ? square : friendlyPiece;
                    attackedSquares.Add(square);
                    break;
                }
                if (checkFunc(piece) && Piece.IsColour(piece, attackingColour))
                {  // case 2 + 5
                    attackedSquares.Add(undefendedAttacker);  // kinda complicated but has a point
                    undefendedAttacker = foundAttacker != -1 ? square : undefendedAttacker;
                    attackedSquares.Add(undefendedAttacker);
                    foundAttacker = square;
                    undefendedAttacker = square;
                }
                if (Piece.IsColour(piece, attackingColour) && foundAttacker == -1)
                {  // case 3
                    attackedSquares.Clear();
                    attackedSquares.Add(square);
                }
                if (Piece.IsColour(piece, attackingColour) && foundAttacker != -1 && !checkFunc(piece))
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
            if (foundAttacker != -1 && friendlyPiece != -1)
            {
                pins.Add(new Pin(friendlyPiece, direction));
            }
            if (foundAttacker != -1 && attackedSquares.Contains(kingSquare))
            {
                inDoubleCheck = inCheck != -1 ? true : false;
                inCheck = foundAttacker;
            }
            if (foundAttacker != -1)
            {
                attackMap.UnionWith(attackedSquares);
            }
            attackedSquares.Clear();
        }
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
                if (0 > attackedSquare || attackedSquare >= 64 || Math.Abs(knight % 8 - (knight + direction) % 8) > 2)
                {
                    continue;
                }
                if (attackedSquare == kingSquare)
                {
                    inDoubleCheck = inCheck != -1 ? true : false;
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
            int square = piece;
            while (0 <= square + dir && square + dir < 64)
            {
                if (Math.Abs(square % 8 - (square + dir) % 8) > 1)
                {
                    break;
                }
                square += dir;
                attackMap.Add(square);
                if (Piece.None != board.Squares[square] && square != kingSquare)
                {
                    break;
                }
            }
        }
    }
    private bool InBounds(int position)
    {
        return 0 <= position && position < 64;
    }
}
