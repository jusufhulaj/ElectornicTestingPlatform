using ElectronicTestingSystem.Data.Repository.IRepository;

namespace ElectronicTestingSystem.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        public IElectronicTestingSystemRepository<TEntity> Repository<TEntity>() where TEntity : class;
        bool Complete();
    }
}
