using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace agile_api.Models;

public class InvitacionPendiente
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	public string EmailInvitado { get; set; } = "";

	[ForeignKey("Tienda")]
	public int TiendaId { get; set; }
	public virtual Tienda Tienda { get; set; } = null!;

	public DateTime FechaInvitacion { get; set; } = DateTime.UtcNow;
}