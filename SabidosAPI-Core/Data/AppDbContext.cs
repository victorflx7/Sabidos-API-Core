using SabidosAPI_Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SabidosAPI_Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resumo> Resumos => Set<Post>();
    public DbSet<Evento> Eventos => Set<Post>();
    public DbSet<Pomodoro> Pomodoros => Set<Post>();
    public DbSet<Flashcard> Flashcards => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.FirebaseUid).IsUnique();
        base.OnModelCreating(modelBuilder);
    }
}
  