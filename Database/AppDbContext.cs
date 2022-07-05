using Microsoft.EntityFrameworkCore;
using ReprodutorMultimia.Database.Entities;

namespace ReprodutorMultimia.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options) {}

    public DbSet<Media> Medias { get; set; }
}