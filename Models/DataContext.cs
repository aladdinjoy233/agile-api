using Microsoft.EntityFrameworkCore;

namespace agile_api.Models;

public class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> options) : base(options) {}

	public DbSet<Usuario> Usuarios { get; set; } = null!;
	public DbSet<Tienda> Tiendas { get; set; } = null!;
	public DbSet<InvitacionPendiente> InvitacionesPendientes { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Tienda>()
			.HasOne(t => t.Dueño)
			.WithOne()
			.HasForeignKey<Tienda>(t => t.DueñoId);

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
	}
}