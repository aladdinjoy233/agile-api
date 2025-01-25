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

	[HttpPost("{tiendaId}/EliminarUsuario/{usuarioId}")]
	[Authorize]
	public async Task<IActionResult> EliminarUsuario(int tiendaId, int usuarioId)
	{
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(t => t.Id == tiendaId);
		if (tienda == null)
		{
			return NotFound("Tienda no encontrada");
		}

		var usuario = await _context.Usuarios.FindAsync(usuarioId);
		if (usuario == null)
		{
			return NotFound("Usuario no encontrado");
		}

		var usuarioActual = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuarioActual == null || tienda.DueñoId != usuarioActual.Id)
		{
			return Unauthorized();
		}

		if (tienda.DueñoId == usuarioId)
		{
			return BadRequest("El dueño de la tienda no puede ser eliminado.");
		}

		if (!tienda.Usuarios.Contains(usuario))
		{
			return BadRequest("El usuario no pertenece a esta tienda.");
		}

		tienda.Usuarios.Remove(usuario);
		await _context.SaveChangesAsync();

		return Ok("Usuario eliminado de la tienda.");
	}

	[HttpPost("{tiendaId}/EliminarInvitacion")]
	[Authorize]
	public async Task<IActionResult> EliminarInvitacion(int tiendaId, [FromBody] InvitacionForm email)
	{
		var tienda = await _context.Tiendas.FindAsync(tiendaId);
		if (tienda == null)
		{
			return NotFound("Tienda no encontrada");
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null)
		{
			return Unauthorized();
		}

		var invitacion = await _context.InvitacionesPendientes.FirstOrDefaultAsync(i => i.TiendaId == tiendaId && i.EmailInvitado == email.Email);
		if (invitacion == null)
		{
			return NotFound("Invitación no encontrada");
		}

		_context.InvitacionesPendientes.Remove(invitacion);
		await _context.SaveChangesAsync();

		return Ok("Invitación eliminada.");
	}

	[HttpGet("{tiendaId}/EsDuenio")]
	[Authorize]
	public async Task<IActionResult> EsDuenio(int tiendaId)
	{
		var tienda = await _context.Tiendas.FindAsync(tiendaId);
		if (tienda == null)
		{
			return NotFound("Tienda no encontrada");
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null)
		{
			return Unauthorized();
		}

		return Ok(tienda.DueñoId == usuario.Id);
	}

	[HttpGet("{tiendaId}/obtenerusuarios")]
	[Authorize]
	public async Task<IActionResult> ObtenerUsuarios(int tiendaId)
	{
		var tienda = await _context.Tiendas.Include(t => t.Usuarios).FirstOrDefaultAsync(t => t.Id == tiendaId);
		if (tienda == null)
		{
			return NotFound("Tienda no encontrada");
		}

		var usuario = User.Identity != null ? await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == User.Identity.Name) : null;
		if (usuario == null || !tienda.Usuarios.Contains(usuario))
		{
			return Unauthorized();
		}

		var usuariosConPropiedad = tienda.Usuarios.Select(u => new
		{
			u.Id,
			u.Nombre,
			u.Email,
			EsDuenio = u.Id == tienda.DueñoId,
			EsInvitado = false
		}).ToList();

		var invitacionesPendientes = await _context.InvitacionesPendientes
			.Where(i => i.TiendaId == tiendaId)
			.Select(i => new
			{
				Id = 0, // No hay ID para los invitados pendientes
				Nombre = string.Empty,
				Email = i.EmailInvitado,
				EsDuenio = false,
				EsInvitado = true
			}).ToListAsync();

		usuariosConPropiedad.AddRange(invitacionesPendientes);

		return Ok(usuariosConPropiedad);
	}
}