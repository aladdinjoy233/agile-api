using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace agile_api.Models;

public class Tienda
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[ForeignKey("Usuario")]
	public int DueñoId { get; set; }

	[JsonIgnore]
	public virtual Usuario Dueño { get; set; } = null!;

	public string Nombre { get; set; } = "";
	public string Email { get; set; } = "";
	public string Telefono { get; set; } = "";

	[JsonIgnore]
	public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

	[JsonIgnore]
	public virtual ICollection<InvitacionPendiente> InvitacionesPendientes { get; set; } = new List<InvitacionPendiente>();

	[JsonIgnore]
	public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

	[JsonIgnore]
	public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}