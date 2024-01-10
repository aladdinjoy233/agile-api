using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace agile_api.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HealthController : ControllerBase
	{
		private readonly IConfiguration _configuration;

		public HealthController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpGet]
		public IActionResult CheckHealth()
		{
			try {
				using MySqlConnection connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
				connection.Open();
				return Ok("Successfully connected to the database.");
			}
			catch (Exception ex) {
				return StatusCode(500, ex.Message);
			}
		}
	}
}