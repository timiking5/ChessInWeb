namespace DataAccess.Repository;

public class GameRepository : Repository<Game>, IGameRepository
{
    public GameRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public void Update(Game game)
    {
        _dbContext.Games.Update(game);
    }
}
