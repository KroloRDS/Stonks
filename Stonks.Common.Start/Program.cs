using System.Reflection;
using Microsoft.EntityFrameworkCore;

using Stonks.Data;
using Stonks.Util;
using Stonks.Data.Models;
using Stonks.CQRS.Helpers;

var builder = WebApplication.CreateBuilder(args);
AddServices(builder);
var app = builder.Build();
ConfigureApp(app);
app.Run();

static void AddServices(WebApplicationBuilder builder)
{
	var services = builder.Services;
	services.AddScoped<IStonksLogger, StonksLogger>();
	services.AddScoped<IStonksConfiguration, StonksConfiguration>();
	services.AddScoped<IAddPublicOffers, AddPublicOffers>();
	services.AddScoped<IGiveMoney, GiveMoney>();
	services.AddScoped<ITransferShares, TransferShares>();

	services.AddMediatR(config => config
		.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

	services.AddEndpointsApiExplorer();
	services.AddSwaggerGen();
	AddDb(builder);

	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
	builder.Services.AddDefaultIdentity<User>(options =>
		options.SignIn.RequireConfirmedAccount = true)
		.AddEntityFrameworkStores<AppDbContext>();
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
	app.UseSwagger();
	app.UseSwaggerUI();

	app.MapControllers();
	app.UseHttpsRedirection()
		.UseRouting()
		.UseAuthentication()
		.UseAuthorization()
		.UseStaticFiles();

	app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}");
	app.MapRazorPages();

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
