using Microsoft.EntityFrameworkCore;

namespace agile_api.Models;

public class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> options) : base(options) {}

	public DbSet<Usuario> Usuarios { get; set; } = null!;
}