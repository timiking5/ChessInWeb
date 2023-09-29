namespace ChessLogic;

public class PieceList
{
    public int[] OccupiedSquares;
    private int[] map = new int[64];
    private int numPieces = 0;
    public int Count { get { return numPieces; } }
    public PieceList(int pieceCount = 16)
    {
        OccupiedSquares = new int[pieceCount];
    }
    public void AddPieceAtSquare(int square)
    {
        OccupiedSquares[numPieces] = square;
        map[square] = numPieces;
        numPieces++;
    }
    public void RemovePieceAtSquare(int square)
    {
        int pieceIndex = map[square];  // get the index of this element in the occupiedSquares array
        OccupiedSquares[pieceIndex] = OccupiedSquares[numPieces - 1];  // move last element in array to the place of the removed element
        map[OccupiedSquares[pieceIndex]] = pieceIndex;  // update map to point to the moved element's new location in the array
        numPieces--;
    }
    public void MovePiece(int startSquare, int targetSquare)
    {
        int pieceIndex = map[startSquare];  // get the index of this element in the occupiedSquares array
        OccupiedSquares[pieceIndex] = targetSquare;
        map[targetSquare] = pieceIndex;
    }
    public int this[int index] => OccupiedSquares[index];
}
