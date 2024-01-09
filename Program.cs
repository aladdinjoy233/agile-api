
using agile_api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Habilitar al celu conectarse con la API
// IPV4 URL: 192.168.0.108:5200
builder.WebHost.UseUrls("http://localhost:5200", "http://*:5200");

// Add services to the container.
builder.Services.AddControllers();

//// string secretKey = configuration["TokenAuthentication:SecretKey"] ?? throw new ArgumentNullException(nameof(secretKey));
//// var signingKey = secretKey != null ? new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)) : null;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MySql connection
builder.Services.AddDbContext<DataContext>(
	options => options.UseMySql(
		configuration.GetConnectionString("DefaultConnection"),
		ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
	)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
// 	app.UseSwagger();
// 	app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

// Habilitar CORS
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// app.UseStaticFiles();

// app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
