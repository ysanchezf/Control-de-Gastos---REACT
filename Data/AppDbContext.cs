using Control_de_Gastos.Models;
using Microsoft.EntityFrameworkCore;
namespace Control_de_Gastos.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<AppUser> Users => Set<AppUser>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<AppUser>().HasIndex(u => u.NormalizedUserName).IsUnique();
        model.Entity<Categoria>().HasOne<AppUser>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Gasto>().HasOne<AppUser>().WithMany().HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Categoria>().HasIndex(c => new { c.UserId, c.Nombre });
    }
}
