using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Stonks.Managers;
using Stonks.Models;

namespace Stonks.Controllers;

public class HomeController : Controller
{
	private readonly ILogManager _logManager;

	public HomeController(ILogManager logManager)
	{
		_logManager = logManager;
	}

	public IActionResult Index()
	{
		_logManager.Log("test");
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