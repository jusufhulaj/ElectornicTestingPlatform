using ElectronicTestingSystem.Data.Repository;
using ElectronicTestingSystem.Data.Repository.IRepository;
using System.Collections;

namespace ElectronicTestingSystem.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ElectronicTestingSystemDbContext _dbContext;

        private Hashtable _repositories;

        public UnitOfWork(ElectronicTestingSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IElectronicTestingSystemRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if(_repositories == null )
            {
                _repositories = new Hashtable();
            }

            var type = typeof(TEntity).Name;
            if(!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(ElectronicTestingSystemRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dbContext);

                _repositories.Add(type, repositoryInstance);
            }

            return (IElectronicTestingSystemRepository<TEntity>)_repositories[type];
        }

        public bool Complete()
        {
            var numberOfAffectedRows = _dbContext.SaveChanges();
            return numberOfAffectedRows > 0;
        }
    }
}
