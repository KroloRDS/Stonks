using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stonks.Views.Models;

namespace Stonks.Controllers;

public class HomeController : Controller
{
	public HomeController()
	{
	}

	public IActionResult Index()
	{
		return View();
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, 
		Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		return View(new ErrorViewModel{ RequestId = requestId });
	}
}