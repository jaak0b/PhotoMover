using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Domain;

public class Database : DbContext
{
    public Database()
    {
    }

    public Database(DbContextOptions<Database> options) : base(options)
    {
    }

    public virtual DbSet<TaskModel> Tasks => Set<TaskModel>();

    public virtual DbSet<ConfigurationModel> Configurations => Set<ConfigurationModel>();

    public virtual DbSet<FtpConfigurationModel> FtpConfigurations => Set<FtpConfigurationModel>();

    public FtpConfigurationModel? FtpConfiguration => FtpConfigurations.SingleOrDefault();
}