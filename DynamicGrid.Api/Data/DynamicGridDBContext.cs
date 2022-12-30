using Domain;
using Microsoft.EntityFrameworkCore;

namespace ODataFull.Data;

public class DynamicGridDBContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    public DynamicGridDBContext(DbContextOptions<DynamicGridDBContext> options)
        : base(options)
    {
    }
}
