namespace DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    public IGameRepository Game { get; private set; }
    private readonly ApplicationDbContext _dbContext;
    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        Game = new GameRepository(dbContext);
    }

    public void Save()
    {
        _dbContext.SaveChanges();
    }
}
