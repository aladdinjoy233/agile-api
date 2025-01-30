using agile_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace agile_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
	private readonly DataContext _context;

	public ProductosController(DataContext context)
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

		var productos = await _context.Productos.Where(x => x.TiendaId == tiendaId).ToListAsync();

		return Ok(productos);
	}

	[HttpPost("Crear/{tiendaId}")]
	[Authorize]
	public async Task<IActionResult> Crear(int tiendaId, [FromBody] ProductoForm form)
	{
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(x => x.Id == tiendaId);
		if (tienda == null) {
			return NotFound();
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || !tienda.Usuarios.Contains(usuario)) {
			return Unauthorized();
		}

		var producto = new Producto
		{
			Codigo = form.Codigo,
			Nombre = form.Nombre,
			Precio = form.Precio,
			Stock = form.Stock,
			CategoriaId = form.CategoriaId,
			TiendaId = tiendaId,
		};

		await _context.Productos.AddAsync(producto);
		await _context.SaveChangesAsync();

		return Ok(producto);
	}
}

public class ProductoForm
{
	public string Codigo { get; set; } = "";
	public string Nombre { get; set; } = "";
	public decimal Precio { get; set; }
	public int Stock { get; set; }
	public int? CategoriaId { get; set; }
}