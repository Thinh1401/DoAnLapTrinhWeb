using Microsoft.AspNetCore.Mvc;

namespace ThinhVaKhoa.Controllers
{
	public class ContactController : Controller
	{
		public IActionResult Index()
		{
			return View("Index");
		}
	}
}
