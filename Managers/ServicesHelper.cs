namespace Stonks.Managers
{
	public class ServicesHelper
	{
		public static void AddServices(WebApplicationBuilder builder)
		{
			var services = builder.Services;

			services.AddScoped<ILogManager, LogManager>();
		}
	}
}
