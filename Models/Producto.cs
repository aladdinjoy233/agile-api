using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace agile_api.Models;

public class Producto
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int ProductoId { get; set; }

	[ForeignKey("Tienda")]
	public int TiendaId { get; set; }

	public string Codigo { get; set; } = "";
	public string Nombre { get; set; } = "";

	[Column(TypeName = "decimal(18,2)")]
	public decimal Precio { get; set; }

	public int Stock { get; set; }

	[ForeignKey("Categoria")]
	public int? CategoriaId { get; set; }

	[JsonIgnore]
	public virtual Tienda Tienda { get; set; } = null!;

	[JsonIgnore]
	public virtual Categoria Categoria { get; set; } = null!;
}