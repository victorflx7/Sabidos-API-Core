using SabidosAPI_Core.Models;
using Microsoft.EntityFrameworkCore;

namespace SabidosAPI_Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<User> Users => Set<User>();
    public virtual DbSet<Resumo> Resumos => Set<Resumo>();
    public virtual DbSet<Evento> Eventos => Set<Evento>();
    public virtual DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public virtual DbSet<Pomodoro> Pomodoros => Set<Pomodoro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<User>().HasIndex(u => u.FirebaseUid).IsUnique();

        modelBuilder.Entity<Evento>()
            .HasOne(e => e.User)
            .WithMany()
            .HasPrincipalKey(u => u.FirebaseUid)
            .HasForeignKey(e => e.AuthorUid);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Resumo>()
           .HasOne(e => e.User)
           .WithMany()
           .HasPrincipalKey(u => u.FirebaseUid)
           .HasForeignKey(e => e.AuthorUid);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Flashcard>()
           .HasOne(e => e.User)
           .WithMany()
           .HasPrincipalKey(u => u.FirebaseUid)
           .HasForeignKey(e => e.AuthorUid);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pomodoro>()
            .HasOne(e => e.User)
            .WithMany()
            .HasPrincipalKey(u => u.FirebaseUid)
            .HasForeignKey(e => e.AuthorUid);

        base.OnModelCreating(modelBuilder);
    }
}

