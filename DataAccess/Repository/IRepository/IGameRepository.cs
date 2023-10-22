namespace DataAccess.Repository.IRepository;

public interface IGameRepository : IRepository<Game>
{
    void Update(Game task);
}
