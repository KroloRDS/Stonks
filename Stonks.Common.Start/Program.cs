using Microsoft.EntityFrameworkCore;
using Stonks.Administration.WebApi;
using Stonks.Auth.WebApi;
using Stonks.Common.Db;
using Stonks.Common.Utils.Models.Configuration;
using Stonks.Common.Utils.Models.Constants;
using Stonks.Common.Utils.Services;
using Stonks.Trade.WebApi;

var builder = WebApplication.CreateBuilder(args);
AddServices(builder);
var app = builder.Build();
ConfigureApp(app);
app.Run();

static void AddServices(WebApplicationBuilder builder)
{
	var services = builder.Services;
	var jwtConfiguration = GetJwtConfiguration(builder);

	AddDb(builder);
	AddConfigurations(builder)
		.AddSingleton(jwtConfiguration)
		.AddScoped<ILogProvider, DbLogProvider>()
		.AddSingleton<ICurrentTime, CurrentTime>()
		.AddSingleton<IAuthService, AuthService>()
		.AddAdministrationEndpoints()
		.AddAuthEndpoints(jwtConfiguration)
		.AddTradeEndpoints();
}

static JwtConfiguration GetJwtConfiguration(WebApplicationBuilder builder)
{
	var jwtConfiguration = builder.Configuration
		.GetSection("JwtConfiguration")
		.Get<JwtConfiguration>() ??
		throw new Exception("Missing JwtConfiguration in appsettings.js");
	jwtConfiguration.SigningKey =
		Environment.GetEnvironmentVariable(Constants.JWT_SIGNING_KEY) ??
		throw new Exception($"Missing {Constants.JWT_SIGNING_KEY} env variable");

	return jwtConfiguration;
}

static void AddDb(WebApplicationBuilder builder)
{
	var connectionString = GetConnectionString(builder);
	builder.Services.AddDbContext<AppDbContext>(options =>
		options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
	builder.Services.AddDbContext<ReadOnlyDbContext>(options =>
		options.UseSqlServer(connectionString), ServiceLifetime.Transient);
}

static string GetConnectionString(WebApplicationBuilder builder)
{
	var connectionString = builder.Configuration
		.GetConnectionString("DefaultConnection") ??
		throw new Exception("Missing DefaultConnection connection string in appsettings.json");

	var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ??
		throw new Exception("Missing DB_SERVER env variable");
	var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ??
		throw new Exception("Missing DB_PASSWORD env variable");

	connectionString = connectionString.Replace("<DB_SERVER>", dbServer);
	connectionString = connectionString.Replace("<DB_PASSWORD>", dbPassword);

	return connectionString;
}

static IServiceCollection AddConfigurations(WebApplicationBuilder builder)
{
	var battleRoyaleConfiguration = builder.Configuration
		.GetSection("BattleRoyaleConfiguration")
		.Get<BattleRoyaleConfiguration>() ??
		throw new Exception("Missing BattleRoyaleConfiguration in appsettings.json");

	builder.Services.AddSingleton(battleRoyaleConfiguration);
	return builder.Services;
}

static void ConfigureApp(WebApplication app)
{
	app.UseAdministrationEndpoints()
		.UseAuthEndpoints()
		.UseTradeEndpoints();

	UpdateSchema(app);
}

static void UpdateSchema(IApplicationBuilder app)
{
	using var serviceScope = app.ApplicationServices
		.GetRequiredService<IServiceScopeFactory>()
		.CreateScope();

	using var ctx = serviceScope.ServiceProvider.GetService<AppDbContext>();
	ctx?.Database.Migrate();
}
