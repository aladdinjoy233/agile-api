using agile_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace agile_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TiendasController : ControllerBase
{
	private readonly DataContext _context;
	private readonly IConfiguration _config;

	public TiendasController(DataContext context, IConfiguration configuration)
		{
			_context = context;
			_config = configuration;
		}

	[HttpPost("Crear")]
	[Authorize]
	public async Task<IActionResult> Crear(TiendaForm tiendaNueva)
	{
		var usuario = User.Identity != null ? _context.Usuarios.FirstOrDefault(x => x.Email == User.Identity.Name) : null;

		if (usuario == null) {
			return Unauthorized();
		}

		var tienda = new Tienda
		{
			Nombre = tiendaNueva.Nombre,
			Email = tiendaNueva.Email,
			Telefono = tiendaNueva.Telefono,
			DueñoId = usuario.Id
		};

		await _context.Tiendas.AddAsync(tienda);
		await _context.SaveChangesAsync();

		return Ok();
	}
}