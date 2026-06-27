namespace CinemaBooking.Persistence.Repositories;

using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(params string[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includes)
            query = query.Include(include);
        return await query.ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id, params string[] includes)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        foreach (var include in includes)
            query = query.Include(include);
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }
}
