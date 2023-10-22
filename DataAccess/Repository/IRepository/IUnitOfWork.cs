namespace DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    IGameRepository Game { get; }
    void Save();
}
