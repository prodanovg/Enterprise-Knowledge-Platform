using System.Linq.Expressions;
using Domain.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Interface;

namespace Repository.Implementation;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<T> _entities;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public Task<T> InsertAsync(T entity)
    {
        _entities.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<T> UpdateAsync(T entity)
    {
        _entities.Update(entity);
        return Task.FromResult(entity);
    }

    public Task<T> DeleteAsync(T entity)
    {
        _entities.Remove(entity);
        return Task.FromResult(entity);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public async Task<E?> GetAsync<E>(
        Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (include != null)
        {
            query = include(query);
        }

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query
            .Select(selector)
            .FirstOrDefaultAsync();
    }

    public async Task<List<E>> GetAllAsync<E>(
        Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        int? take = null)
    {
        IQueryable<T> query = _entities;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (include != null)
        {
            query = include(query);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return await query
            .Select(selector)
            .ToListAsync();
    }

    public async Task<PaginatedResult<E>> GetAllPagedAsync<E>(
        Expression<Func<T, E>> selector,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (include != null)
        {
            query = include(query);
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        return new PaginatedResult<E>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<List<TResult>> AggregateAsync<TKey, TResult>(
        Expression<Func<T, TKey>> groupBy,
        Expression<Func<IGrouping<TKey, T>, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query
            .GroupBy(groupBy)
            .Select(selector)
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate)
    {
        return _entities.AnyAsync(predicate);
    }
}