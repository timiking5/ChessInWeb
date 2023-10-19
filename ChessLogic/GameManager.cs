namespace ChessLogic;

public class GameManager
{
    private Board _board { get; set; }
    private MoveGenerator _moveGenerator { get; set; }
    private List<Move> _currentMoves;
    public Dictionary<int, List<Move>> MovesStorage { get; private set; }
    public int[] Squares { get; private set; }
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
    public bool CheckDraw()
    {
        return false;
    }
    public void MakeMove(Move move)
    {
        _board.MakeMove(move);
    }
}
