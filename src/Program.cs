using Microsoft.EntityFrameworkCore;
using Stonks.Data;
using Stonks.Models;
using Stonks.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration
	.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(connectionString));

ServicesHelper.AddServices(builder);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options =>
	options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();
UpdateSchema(app);

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

app.Run();

static void UpdateSchema(IApplicationBuilder app)
{
	using var serviceScope = app.ApplicationServices
		.GetRequiredService<IServiceScopeFactory>()
		.CreateScope();
	using var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
	
	if (context == null) return;
	
	context.Database.Migrate();
}