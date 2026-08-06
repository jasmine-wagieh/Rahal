using Microsoft.EntityFrameworkCore;
using RahalApi.Models;

namespace RahalApi.Data;

public class RahalDbContext : DbContext
{
    public RahalDbContext(DbContextOptions<RahalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Place> Places => Set<Place>();
}