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
}