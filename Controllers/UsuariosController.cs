using agile_api.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace agile_api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsuariosController : ControllerBase
	{
		private readonly DataContext _context;
		private readonly IConfiguration _config;

		public UsuariosController(DataContext context, IConfiguration configuration)
		{
			_context = context;
			_config = configuration;
		}

		// [HttpGet("ObtenerTodos")]
		// public IActionResult ObtenerTodos()
		// {
		// 	try {
		// 		return Ok(_context.Usuarios.ToList());
		// 	}
		// 	catch (Exception ex) {
		// 		return BadRequest(ex.Message);
		// 	}
		// }

		[HttpPost("Signup")]
		public async Task<IActionResult> Signup(Usuario signup)
		{
			// Comprobar que el email ingresado ya no exista como usuario
			var existeUsuarioConEmail = await _context.Usuarios.AnyAsync(x => x.Email == signup.Email);
			if (existeUsuarioConEmail)
			{
				return BadRequest("Ya existe un usuario con este correo");
			}

			// Hashear contraseña
			string hashed = Hashear(signup.Password);

			// Crear y guardar el usuario
			var usuario = new Usuario
			{
				Email = signup.Email,
				Nombre = signup.Nombre,
				Password = hashed
			};
			_context.Usuarios.Add(usuario);
			await _context.SaveChangesAsync();

			// Ver si hay invitaciones pendientes y agregar al usuario a la tienda correspondiente
			var invitacionesPendientes = await _context.InvitacionesPendientes
				.Where(i => i.EmailInvitado == signup.Email)
				.ToListAsync();

			foreach (var invitacion in invitacionesPendientes)
			{
				var tienda = await _context.Tiendas.FindAsync(invitacion.TiendaId);
				if (tienda != null)
				{
					tienda.Usuarios.Add(usuario);
					_context.InvitacionesPendientes.Remove(invitacion);
				}
			}

			await _context.SaveChangesAsync();

			// Devolver JWT
			return Ok(CrearToken(usuario));
		}

		[HttpPost("Login")]
		public IActionResult Login(Usuario login)
		{
			try
			{
				var usuario = _context.Usuarios.FirstOrDefault(x => x.Email == login.Email);

				// Verificar que exista el usuario
				if (usuario == null) {
					return BadRequest("El email o contraseña son incorrectos");
				}

				// Verificar que la contraseña sea correcta
				string hashed = Hashear(login.Password);
				if (hashed != usuario.Password) {
					return BadRequest("El email o contraseña son incorrectos");
				}

				// Devolver JWT
				return Ok(CrearToken(usuario));
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpGet("Obtener")]
		[Authorize]
		public IActionResult Obtener()
		{
			var usuario = User.Identity != null ? _context.Usuarios.FirstOrDefault(x => x.Email == User.Identity.Name) : null;

			if (usuario == null) {
				return Unauthorized();
			}

			return Ok(usuario);
		}

		private string Hashear(string c) {
			return Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: c,
				salt: System.Text.Encoding.ASCII.GetBytes(_config["Salt"] ?? ""),
				prf: KeyDerivationPrf.HMACSHA1,
				iterationCount: 10000,
				numBytesRequested: 256 / 8
			));
		}

		private string CrearToken(Usuario usuario) {

			// Ver que usuario.Id y usuario.Email sean validos
			if (usuario.Id <= 0 || usuario.Email == null) {
				throw new Exception("Usuario invalido");
			}

			var securityKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_config["TokenAuthentication:SecretKey"] ?? ""));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new(ClaimTypes.Name, usuario.Email),
				new("Id", usuario.Id.ToString())
			};

			var token = new JwtSecurityToken
			(
				issuer: _config["TokenAuthentication:Issuer"],
				audience: _config["TokenAuthentication:Audience"],
				claims: claims,
				expires: DateTime.Now.AddDays(20),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}