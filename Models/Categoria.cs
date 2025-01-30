using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace agile_api.Models;

public class Categoria
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int CategoriaId { get; set; }

	public string Nombre { get; set; } = "";

	[ForeignKey("Tienda")]
	public int TiendaId { get; set; }

	[JsonIgnore]
	public virtual Tienda Tienda { get; set; } = null!;

	[JsonIgnore]
	public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}