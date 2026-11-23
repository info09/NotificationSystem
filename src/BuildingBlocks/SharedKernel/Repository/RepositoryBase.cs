using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuildingBlocks.SharedKernel.Repository;

/// <summary>
/// Triển khai repository cơ sở dùng EF Core cho các thao tác dữ liệu chung.
/// </summary>
/// <typeparam name="T">Kiểu thực thể domain.</typeparam>
public class RepositoryBase<T> : IRepository<T> where T : Entity.Entity
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Khởi tạo repository với <see cref="DbContext"/> tương ứng.
    /// </summary>
    /// <param name="context">Ngữ cảnh cơ sở dữ liệu.</param>
    public RepositoryBase(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }
}
