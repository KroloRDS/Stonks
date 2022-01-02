using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stonks.Models;

namespace Stonks.Controllers;

public class HomeController : Controller
{
	public HomeController()
	{
	}

	public IActionResult Index()
	{
		Console.WriteLine(Environment.GetEnvironmentVariable("APPSETTING_BATTLEROYALE_FUN")); 
		return View();
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}