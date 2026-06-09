using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Extensions;
using StridexFinal_CSharp.Models;
using StridexFinal_CSharp.Repositories;

namespace StridexFinal_CSharp.Controllers;

public class DonHangController : Controller
{
    private const string CartKey = "GioHang";
    private readonly DonHangRepository _repo;

    public DonHangController(DonHangRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult ThanhToan()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Url.Action("ThanhToan", "DonHang") });

        var gioHang = LayGioHang();
        if (!gioHang.Any())
        {
            TempData["Error"] = "Giỏ hàng đang rỗng";
            return RedirectToAction("Index", "GioHang");
        }

        return View(new ThanhToanViewModel
        {
            HoTen = HttpContext.Session.GetString("HoTen") ?? "",
            GioHang = gioHang
        });
    }

    [HttpPost]
    public async Task<IActionResult> ThanhToan(ThanhToanViewModel model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");

        var gioHang = LayGioHang();
        model.GioHang = gioHang;
        if (!gioHang.Any()) ModelState.AddModelError(string.Empty, "Giỏ hàng đang rỗng");
        if (!ModelState.IsValid) return View(model);

        var donHangId = await _repo.TaoDonHangAsync(userId.Value, gioHang);
        HttpContext.Session.Remove(CartKey);
        TempData["Success"] = "Đặt hàng thành công";
        return RedirectToAction(nameof(ChiTiet), new { id = donHangId });
    }

    public async Task<IActionResult> LichSu()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("DangNhap", "TaiKhoan");
        var list = await _repo.GetByNguoiDungAsync(userId.Value);
        return View(list);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var donHang = await _repo.GetDetailAsync(id);
        if (donHang == null) return NotFound();

        var userId = HttpContext.Session.GetInt32("UserId");
        var vaiTro = HttpContext.Session.GetString("VaiTro");
        if (!string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase) && donHang.NguoiDungId != userId)
            return RedirectToAction("DangNhap", "TaiKhoan");

        return View(donHang);
    }

    private List<GioHangItem> LayGioHang() => HttpContext.Session.GetObject<List<GioHangItem>>(CartKey) ?? new();
}
