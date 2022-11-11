using MediatR;
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
	services.AddMediatR(Assembly.GetExecutingAssembly());
	builder.Services.AddControllersWithViews();

	var connectionString = builder.Configuration
		.GetConnectionString("DefaultConnection");
	services.AddDbContext<AppDbContext>(options => 
		options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
	services.AddDbContext<ReadOnlyDbContext>(options =>
		options.UseSqlServer(connectionString), ServiceLifetime.Transient);

	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
	builder.Services.AddDefaultIdentity<User>(options =>
		options.SignIn.RequireConfirmedAccount = true)
		.AddEntityFrameworkStores<AppDbContext>();
}

static void ConfigureApp(WebApplication app)
{
	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseMigrationsEndPoint();
	}
	else
	{
		app.UseExceptionHandler("/Home/Error");
		// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
		app.UseHsts();
	}

	app.UseHttpsRedirection();
	app.UseStaticFiles();
	app.UseRouting();
	app.UseAuthentication();
	app.UseAuthorization();

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
