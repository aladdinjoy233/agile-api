using agile_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace agile_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
	private readonly DataContext _context;

	public CategoriasController(DataContext context)
	{
		_context = context;
	}

	[HttpGet("Listar/{tiendaId}")]
	[Authorize]
	public async Task<IActionResult> Listar(int tiendaId)
	{
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(x => x.Id == tiendaId);
		if (tienda == null) {
			return NotFound();
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || !tienda.Usuarios.Contains(usuario)) {
			return Unauthorized();
		}

		var categorias = await _context.Categorias.Where(x => x.TiendaId == tiendaId).ToListAsync();

		return Ok(categorias);
	}

	[HttpPost("Crear/{tiendaId}")]
	[Authorize]
	public async Task<IActionResult> Crear(int tiendaId, [FromBody] CategoriaForm categoriaNueva)
	{
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(x => x.Id == tiendaId);
		if (tienda == null) {
			return NotFound();
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || !tienda.Usuarios.Contains(usuario)) {
			return Unauthorized();
		}

		var categoria = new Categoria
		{
			Nombre = categoriaNueva.Nombre,
			TiendaId = tiendaId,
		};

		await _context.Categorias.AddAsync(categoria);
		await _context.SaveChangesAsync();

		return Ok(categoria);
	}

}

public class CategoriaForm
{
	public string Nombre { get; set; } = "";
}