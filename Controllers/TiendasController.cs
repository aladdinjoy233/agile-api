using agile_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null) {
			return Unauthorized();
		}

		var tienda = new Tienda
		{
			Nombre = tiendaNueva.Nombre,
			Email = tiendaNueva.Email,
			Telefono = tiendaNueva.Telefono,
			DueñoId = usuario.Id,
			Usuarios = new List<Usuario> { usuario } // Agregar al dueño como usuario de la tienda
		};

		await _context.Tiendas.AddAsync(tienda);
		await _context.SaveChangesAsync();

		return Ok();
	}

	[HttpPut("Editar/{id}")]
	[Authorize]
	public async Task<IActionResult> Editar(int id, TiendaForm tiendaActualizada)
	{
		var tienda = await _context.Tiendas.FindAsync(id);
		if (tienda == null)
		{
			return NotFound();
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || tienda.DueñoId != usuario.Id)
		{
			return Unauthorized();
		}

		tienda.Nombre = tiendaActualizada.Nombre;
		tienda.Email = tiendaActualizada.Email;
		tienda.Telefono = tiendaActualizada.Telefono;

		await _context.SaveChangesAsync();

		return Ok(tienda);
	}

	[HttpDelete("Eliminar/{id}")]
	[Authorize]
	public async Task<IActionResult> Eliminar(int id)
	{
		var tienda = await _context.Tiendas.FindAsync(id);
		if (tienda == null)
		{
			return NotFound();
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || tienda.DueñoId != usuario.Id)
		{
			return Unauthorized();
		}

		_context.Tiendas.Remove(tienda);
		await _context.SaveChangesAsync();

		return Ok();
	}

	[HttpGet("ObtenerLista")]
	[Authorize]
	public async Task<IActionResult> ObtenerLista()
	{
		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null)
		{
			return Unauthorized();
		}

		var tiendas = await _context.Tiendas
			.Where(t => t.Usuarios.Contains(usuario))
			.ToListAsync();

		return Ok(tiendas);
	}
}