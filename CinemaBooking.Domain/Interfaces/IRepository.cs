namespace CinemaBooking.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, params string[] includes);
    Task<IReadOnlyList<TEntity>> GetAllAsync(params string[] includes);
    Task<TEntity> AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}
