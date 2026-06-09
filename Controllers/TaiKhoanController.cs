using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Models;
using StridexFinal_CSharp.Repositories;

namespace StridexFinal_CSharp.Controllers;

public class TaiKhoanController : Controller
{
    private readonly NguoiDungRepository _repo;

    public TaiKhoanController(NguoiDungRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new DangNhapViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _repo.DangNhapAsync(model.Email, model.MatKhau);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("HoTen", user.HoTen);
        HttpContext.Session.SetString("VaiTro", user.VaiTro);
        TempData["Success"] = $"Xin chào {user.HoTen}!";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return user.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult DangKy() => View(new DangKyViewModel());

    [HttpPost]
    public async Task<IActionResult> DangKy(DangKyViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _repo.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email đã tồn tại");
            return View(model);
        }

        await _repo.RegisterAsync(model);
        TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(DangNhap));
    }

    public IActionResult DangXuat()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Đã đăng xuất";
        return RedirectToAction("Index", "Home");
    }
}
