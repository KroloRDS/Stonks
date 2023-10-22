using Microsoft.AspNetCore.Mvc;

namespace Stonks.Controllers;

public class HomeController : Controller
{
	public IActionResult Index()
	{
		return View();
	}
}