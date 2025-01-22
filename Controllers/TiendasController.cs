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

	[HttpGet("Obtener/{id}")]
	[Authorize]
	public async Task<IActionResult> Obtener(int id)
	{
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(t => t.Id == id);
		if (tienda == null)
		{
			return NotFound();
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || !tienda.Usuarios.Contains(usuario))
		{
			return Unauthorized();
		}

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

	[HttpPost("{tiendaId}/InvitarUsuario")]
	[Authorize]
	public async Task<IActionResult> InvitarUsuario(int tiendaId, [FromBody] InvitacionForm invitacion)
	{
		if (string.IsNullOrWhiteSpace(invitacion.Email))
		{
			return BadRequest("El correo electrónico es requerido.");
		}

		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(t => t.Id == tiendaId);

		if (tienda == null)
		{
			return NotFound("Tienda no encontrada");
		}

		var usuarioInvitado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == invitacion.Email);
		if (usuarioInvitado != null)
		{
			if (!tienda.Usuarios.Contains(usuarioInvitado))
			{
				tienda.Usuarios.Add(usuarioInvitado);
				await _context.SaveChangesAsync();
				return Ok("Usuario agregado a la tienda.");
			}

			return BadRequest("El usuario ya se encuentra en la tienda.");
		}

		var invitacionExistente = await _context.InvitacionesPendientes.FirstOrDefaultAsync(i => i.TiendaId == tiendaId && i.EmailInvitado == invitacion.Email);
		if (invitacionExistente != null)
		{
			return BadRequest("La invitación ya existe.");
		}

		var nuevaInvitacion = new InvitacionPendiente
		{
			EmailInvitado = invitacion.Email,
			TiendaId = tiendaId
		};

		await _context.InvitacionesPendientes.AddAsync(nuevaInvitacion);
		await _context.SaveChangesAsync();

		return Ok("Invitación enviada");
	}

	[HttpPost("{tiendaId}/Salir")]
	[Authorize]
	public async Task<IActionResult> Salir(int tiendaId)
	{
		// var tienda = await _context.Tiendas.FindAsync(tiendaId); // No incluye a los usuarios
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(t => t.Id == tiendaId);
		if (tienda == null)
		{
			return NotFound("Tienda no encontrada");
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null)
		{
			return Unauthorized();
		}

		if (tienda.DueñoId == usuario.Id)
		{
			return BadRequest("El dueño de la tienda no puede salir de la tienda.");
		}

		if (!tienda.Usuarios.Contains(usuario))
		{
			return BadRequest("El usuario no pertenece a esta tienda.");
		}

		tienda.Usuarios.Remove(usuario);
		await _context.SaveChangesAsync();

		return Ok("Has salido de la tienda.");
	}
}