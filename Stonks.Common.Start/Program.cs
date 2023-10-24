using Microsoft.EntityFrameworkCore;
using Stonks.Administration.WebApi;
using Stonks.Common.Db;
using Stonks.Common.Utils;
using Stonks.Trade.WebApi;

var builder = WebApplication.CreateBuilder(args);
AddServices(builder);
var app = builder.Build();
ConfigureApp(app);
app.Run();

static void AddServices(WebApplicationBuilder builder)
{
	var services = builder.Services;
	services.AddScoped<ILogProvider, DbLogProvider>()
		.AddAdministrationEndpoints()
		.AddTradeEndpoints();
	AddDb(builder);
}

static void AddDb(WebApplicationBuilder builder)
{
	var connectionString = GetConnectionString(builder);
	builder.Services.AddDbContext<AppDbContext>(options =>
		options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
	builder.Services.AddDbContext<ReadOnlyDbContext>(options =>
		options.UseSqlServer(connectionString), ServiceLifetime.Transient);
}

static void ConfigureApp(WebApplication app)
{
	app.UseAdministrationEndpoints()
		.UseTradeEndpoints();
}

static void UpdateSchema(IApplicationBuilder app)
{
	using var serviceScope = app.ApplicationServices
		.GetRequiredService<IServiceScopeFactory>()
		.CreateScope();

	using var ctx = serviceScope.ServiceProvider.GetService<AppDbContext>();
	ctx?.Database.Migrate();
}

static string GetConnectionString(WebApplicationBuilder builder)
{
	var connectionString = builder.Configuration
		.GetConnectionString("DefaultConnection") ?? string.Empty;
	var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ??
		string.Empty;
	var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ??
		string.Empty;
	connectionString = connectionString.Replace("<DB_SERVER>", dbServer);
	connectionString = connectionString.Replace("<DB_PASSWORD>", dbPassword);
	return connectionString;
}
