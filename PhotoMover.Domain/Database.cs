using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain
{
  public class Database : DbContext
  {
    public Database()
    {
    }

    public Database(DbContextOptions<Database> options) : base(options)
    {
    }

    public virtual DbSet<TaskModel> Tasks => Set<TaskModel>();

    public event EventHandler CollectionChanged;

    override public EntityEntry<TEntity> Add<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = Set<TEntity>().Add(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public void AddRange<TEntity>(IEnumerable<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().AddRange(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = await Set<TEntity>().AddAsync(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public void InvokeCollectionChanged()
    {
      CollectionChanged?.Invoke(this, new());
    }

    override public EntityEntry<TEntity> Remove<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> value = Set<TEntity>().Remove(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
      return value;
    }

    public async Task<EntityEntry<TEntity>> RemoveAsync<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> value = Set<TEntity>().Remove(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
      return value;
    }

    public void RemoveRange<TEntity>(List<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().RemoveRange(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
    }

    override public EntityEntry<TEntity> Update<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = Set<TEntity>().Update(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public async Task<EntityEntry<TEntity>> UpdateAsync<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = Set<TEntity>().Update(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public void UpdateRange<TEntity>(List<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().UpdateRange(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
    }

    public async Task UpdateRangeAsync<TEntity>(List<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().UpdateRange(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
    }
  }
}