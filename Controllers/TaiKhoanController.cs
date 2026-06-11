using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Models;
using StridexFinal_CSharp.Repositories;
using System.Security.Claims;

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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return user.VaiTro.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult DangNhapGoogle(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(
            nameof(GoogleCallback),
            "TaiKhoan",
            new { returnUrl }
        );

        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        properties.SetParameter("prompt", "select_account");

        return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme
        );
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
    {
        var result = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        if (!result.Succeeded)
        {
            TempData["Error"] = "Đăng nhập Google thất bại";
            return RedirectToAction(nameof(DangNhap));
        }

        var email = result.Principal?
            .FindFirst(ClaimTypes.Email)?
            .Value;

        var hoTen = result.Principal?
            .FindFirst(ClaimTypes.Name)?
            .Value ?? email;

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Không lấy được email từ Google";
            return RedirectToAction(nameof(DangNhap));
        }

        var user = await _repo.GetByEmailAsync(email);

        if (user == null)
        {
            user = await _repo.RegisterGoogleAsync(
                hoTen ?? "Google User",
                email
            );
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("HoTen", user.HoTen);
        HttpContext.Session.SetString("VaiTro", user.VaiTro);

        TempData["Success"] = $"Xin chào {user.HoTen}!";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult DangKy()
    {
        return View(new DangKyViewModel());
    }

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

    public async Task<IActionResult> DangXuat()
    {
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        TempData["Success"] = "Đã đăng xuất";
        return RedirectToAction("Index", "Home");
    }
}