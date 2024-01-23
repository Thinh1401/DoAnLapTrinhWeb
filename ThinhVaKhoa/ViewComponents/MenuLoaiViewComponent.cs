using ThinhVaKhoa.Models;
using ThinhVaKhoa.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.ViewComponents
{
	public class MenuLoaiViewComponent : ViewComponent
	{
		private readonly ThinhVaKhoaContext db;

		public MenuLoaiViewComponent(ThinhVaKhoaContext context) => db = context;

		public IViewComponentResult Invoke()
		{
			var data = db.Loais.Select(lo => new MenuLoaiVM
			{
				MaLoai = lo.MaLoai,
				TenLoai = lo.TenLoai,
				SoLuong = lo.HangHoas.Count
			}).OrderBy(p => p.TenLoai);

			return View(data); // Default.cshtml
			//return View("Default", data);
		}
	}
}
