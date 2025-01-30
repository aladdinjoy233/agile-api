using Microsoft.EntityFrameworkCore;

namespace agile_api.Models;

public class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> options) : base(options) {}

	public DbSet<Usuario> Usuarios { get; set; } = null!;
	public DbSet<Tienda> Tiendas { get; set; } = null!;
	public DbSet<InvitacionPendiente> InvitacionesPendientes { get; set; } = null!;
	public DbSet<Categoria> Categorias { get; set; } = null!;
	public DbSet<Producto> Productos { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Tienda>()
			.HasOne(t => t.Dueño)
			.WithMany()
			.HasForeignKey(t => t.DueñoId)
			.OnDelete(DeleteBehavior.Restrict);

		modelBuilder.Entity<Usuario>()
			.HasMany(u => u.Tiendas)
			.WithMany(t => t.Usuarios)
			.UsingEntity<Dictionary<string, object>>(
				"UsuarioTienda",
				j => j.HasOne<Tienda>().WithMany().HasForeignKey("TiendaId"),
				j => j.HasOne<Usuario>().WithMany().HasForeignKey("UserId")
			);

		modelBuilder.Entity<InvitacionPendiente>()
			.HasOne(i => i.Tienda)
			.WithMany(t => t.InvitacionesPendientes)
			.HasForeignKey(i => i.TiendaId);

		modelBuilder.Entity<Categoria>()
			.HasOne(c => c.Tienda)
			.WithMany(t => t.Categorias)
			.HasForeignKey(c => c.TiendaId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<Producto>()
			.HasOne(p => p.Tienda)
			.WithMany(t => t.Productos)
			.HasForeignKey(p => p.TiendaId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<Producto>()
			.HasOne(p => p.Categoria)
			.WithMany(c => c.Productos)
			.HasForeignKey(p => p.CategoriaId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}