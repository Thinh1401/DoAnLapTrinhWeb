﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThinhVaKhoa.Models;
using ThinhVaKhoa.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ThinhVaKhoa.Helpers;


namespace ThinhVaKhoa.Controllers
{
    public class CartController : Controller
    {
        private readonly ThinhVaKhoaContext db;

        public CartController(ThinhVaKhoaContext context)
        {
            db = context;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }

            return View(Cart);
        }


        [HttpPost]
        public IActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                var khachHang = new KhachHang();
                if (model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                }

                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,                  
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };

                db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(cthds);
                    db.SaveChanges();

                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }
                catch
                {
                    db.Database.RollbackTransaction();
                }
            }

            return View(Cart);
        }
    }
}
