using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace agile_api.Models;

public class Tienda
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[ForeignKey("Usuario")]
	public int DueñoId { get; set; }
	public virtual Usuario Dueño { get; set; } = null!;
	public string Nombre { get; set; } = "";
	public string Email { get; set; } = "";
	public string Telefono { get; set; } = "";

	public List<Usuario> Usuarios { get; set; } = [];
}