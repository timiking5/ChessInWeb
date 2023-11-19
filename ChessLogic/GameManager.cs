namespace ChessLogic;

public class GameManager
{
    private Board _board { get; set; }
    private MoveGenerator _moveGenerator { get; set; }
    private List<Move> _currentMoves;
    public Dictionary<int, List<Move>> MovesStorage { get; private set; }
    public int[] Squares { get; private set; }
    public bool WhiteToMove
    {
        get
        {
            return _board.WhiteToMove;
        }
    }
    public bool InCheck {
        get
        {
            return _moveGenerator.InCheck;
        }
    }
    public GameManager()
    {
        _board = new();
        _board.LoadPositionFromFen(Util.StartingPos);
        Squares = _board.Squares;
        MovesStorage = new();
        _moveGenerator = new(_board);
    }
    public GameManager(Board board)
    {
        Squares = board.Squares;
        MovesStorage = new();
        _board = board;
        _moveGenerator = new(board);
    }
    public List<Move> GetMoves()
    {
        _currentMoves = _moveGenerator.GenerateMoves();
        MovesToFormat();
        return _currentMoves;
    }
    private void MovesToFormat()
    {
        MovesStorage.Clear();
        foreach (var move in _currentMoves)
        {
            if (MovesStorage.ContainsKey(move.StartIndex))
            {
                MovesStorage[move.StartIndex].Add(move);
                continue;
            }
            MovesStorage[move.StartIndex] = new() { move };
        }
    }

    public bool CheckMate()
    {
        return _currentMoves.Count == 0 && _moveGenerator.InCheck;
    }
    public bool CheckCheck()
    {
        return _currentMoves.Count > 0 && _moveGenerator.InCheck;

    }
    public bool CheckThreeFoldRepition()
    {
        return _board.HashedPositionsCount[_board.CurrentPositionHash] >= 3;
    }
    public bool CheckStalemate()
    {
        return _currentMoves.Count == 0 && !_moveGenerator.InCheck;
    }
    public bool CheckInsufficientMaterial()
    {
        if (_board.Queens[0].Count > 0 || _board.Queens[1].Count > 0)
        {
            return false;
        }
        if (_board.Queens[0].Count > 0 || _board.Queens[1].Count > 0)
        {
            return false;
        }
        if (_board.Bishops[0].Count > 1 || _board.Bishops[1].Count > 1)
        {  // 2 bishops checkmate is possible
            return false;
        }
        if (_board.Knights[0].Count > 1 || _board.Knights[1].Count > 1)
        {  // impossible to force checkmate, but a player can blunder it
            return false;
        }
        if (_board.Pawns[0].Count > 0 || _board.Pawns[1].Count > 0)
        {
            return false;
        }
        if (_board.Bishops[0].Count > 0 && _board.Knights[0].Count > 0 ||
            _board.Bishops[1].Count > 0 && _board.Knights[1].Count > 0)
        {  // knight and bishop checkmate
            return false;
        }
        return true;
    }
    public bool CheckFiftyMove()
    {
        return _board.FiftyMoveCounter >= 50;
    }
    public string ReturnGameState()
    {
        switch (true)
        {
            case true when CheckFiftyMove():
                return "Fifty Move Draw.";
            case true when CheckInsufficientMaterial():
                return "Draw due to insufficient material.";
            case true when CheckStalemate():
                return "Stalemate.";
            case true when CheckMate():
                return "Checkmate!";
            case true when CheckCheck():
                return "Check!";
            case true when CheckThreeFoldRepition():
                return "Draw due to repition";
            default:
                return _board.WhiteToMove ? "White's move" : "Black's move";
        }
    }
    public void MakeMove(Move move)
    {
        _board.MakeMove(move);
    }
}
