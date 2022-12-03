using System.Linq.Expressions;

namespace ElectronicTestingSystem.Data.Repository.IRepository
{
    public interface IElectronicTestingSystemRepository<TEntity> where TEntity : class
    {
        void Create(TEntity entity);
        void CreateRange(List<TEntity> entities);

        void Update(TEntity entity);
        void UpdateRange(List<TEntity> entities);

        void Delete(TEntity entity);
        void DeleteRange(List<TEntity> entities);

        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetById(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> GetByCondition(Expression<Func<TEntity, bool>> expression);
    }
}
