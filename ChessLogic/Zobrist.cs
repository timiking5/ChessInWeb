namespace ChessLogic;

public static class Zobrist
{
    private static long[,] randomСoefficients = GenerateRandomNumbers();
    private static Dictionary<int, int> pieceToIndex = new()
    {
        { 9, 0 },
        { 17, 1 },
        { 10, 2 },
        { 18, 3 },
        { 11, 4 },
        { 19, 5 },
        { 13, 6 },
        { 21, 7 },
        { 14, 8 },
        { 22, 9 },
        { 15, 10 },
        { 23, 11 },
    };
    private static long[,] GenerateRandomNumbers()
    {
        var random = new Random();
        long[,] coefficients = new long[64, 12];
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                coefficients[i, j] = random.NextInt64(long.MaxValue);
            }
        }
        return coefficients;
    }
    public static long GetPositionHash(this int[] squares)
    {
        long respond = 0;
        for (int i = 0; i < 64; i++)
        {
            if (squares[i] == Piece.None)
            {
                continue;
            }
            long squareHash = randomСoefficients[i, pieceToIndex[squares[i]]];
            respond = respond == 0 ? squareHash : respond ^ squareHash;
        }
        return respond;
    }
}
