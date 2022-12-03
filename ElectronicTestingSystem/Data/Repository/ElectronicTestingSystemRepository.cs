using ElectronicTestingSystem.Data.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ElectronicTestingSystem.Data.Repository
{
    public class ElectronicTestingSystemRepository<TEntity> : IElectronicTestingSystemRepository<TEntity> where TEntity : class
    {

        private readonly ElectronicTestingSystemDbContext _dbContext;

        public ElectronicTestingSystemRepository(ElectronicTestingSystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        // CRUD
        public void Create(TEntity entity)
        {
            _dbContext.Set<TEntity>().Add(entity);
        }

        public void CreateRange(List<TEntity> entities)
        {
            _dbContext.Set<TEntity>().AddRange(entities);
        }

        public void Delete(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }

        public void DeleteRange(List<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
        }

        public void UpdateRange(List<TEntity> entities)
        {
            _dbContext.Set<TEntity>().UpdateRange(entities);
        }

        // GET Methods
        public IQueryable<TEntity> GetAll()
        {
            var entites = _dbContext.Set<TEntity>();

            return entites;
        
        }

        public IQueryable<TEntity> GetByCondition(Expression<Func<TEntity, bool>> expression)
        {
            var entities = _dbContext.Set<TEntity>().Where(expression);

            return entities;
        }

        public IQueryable<TEntity> GetById(Expression<Func<TEntity, bool>> expression)
        {
            var entity = _dbContext.Set<TEntity>().Where(expression);

            return entity;
        }
    }
}
