using SabidosAPI_Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SabidosAPI_Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resumo> Resumos => Set<Resumo>();
    public DbSet<Evento> Eventos => Set<Evento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<User>().HasIndex(u => u.FirebaseUid).IsUnique();

        modelBuilder.Entity<Evento>()
            .HasOne(e => e.User)
            .WithMany()
            .HasPrincipalKey(u => u.FirebaseUid)
            .HasForeignKey(e => e.AuthorUid);

        base.OnModelCreating(modelBuilder);
    }
}
  