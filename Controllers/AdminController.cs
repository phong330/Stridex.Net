using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Models;
using StridexFinal_CSharp.Repositories;

namespace StridexFinal_CSharp.Controllers;

public class AdminController : Controller
{
    private readonly SanPhamRepository _sanPhamRepo;
    private readonly DonHangRepository _donHangRepo;
    private readonly NguoiDungRepository _nguoiDungRepo;

    public AdminController(SanPhamRepository sanPhamRepo, DonHangRepository donHangRepo, NguoiDungRepository nguoiDungRepo)
    {
        _sanPhamRepo = sanPhamRepo;
        _donHangRepo = donHangRepo;
        _nguoiDungRepo = nguoiDungRepo;
    }

    private bool IsAdmin() => string.Equals(HttpContext.Session.GetString("VaiTro"), "Admin", StringComparison.OrdinalIgnoreCase);
    private IActionResult Denied() => RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Request.Path.ToString() });

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin()) return Denied();
        ViewBag.SoSanPham = (await _sanPhamRepo.GetAllAsync()).Count;
        ViewBag.SoDonHang = (await _donHangRepo.GetAllAsync()).Count;
        ViewBag.SoNguoiDung = (await _nguoiDungRepo.GetAllAsync()).Count;
        return View();
    }

    public async Task<IActionResult> SanPham()
    {
        if (!IsAdmin()) return Denied();
        var list = await _sanPhamRepo.GetAllAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult ThemSanPham()
    {
        if (!IsAdmin()) return Denied();
        return View("FormSanPham", new SanPham());
    }

    [HttpPost]
    public async Task<IActionResult> ThemSanPham(SanPham model)
    {
        if (!IsAdmin()) return Denied();
        if (!ModelState.IsValid) return View("FormSanPham", model);
        await _sanPhamRepo.AddAsync(model);
        TempData["Success"] = "Đã thêm sản phẩm";
        return RedirectToAction(nameof(SanPham));
    }

    [HttpGet]
    public async Task<IActionResult> SuaSanPham(int id)
    {
        if (!IsAdmin()) return Denied();
        var sp = await _sanPhamRepo.GetByIdAsync(id);
        if (sp == null) return NotFound();
        return View("FormSanPham", sp);
    }

    [HttpPost]
    public async Task<IActionResult> SuaSanPham(SanPham model)
    {
        if (!IsAdmin()) return Denied();
        if (!ModelState.IsValid) return View("FormSanPham", model);
        await _sanPhamRepo.UpdateAsync(model);
        TempData["Success"] = "Đã cập nhật sản phẩm";
        return RedirectToAction(nameof(SanPham));
    }

    public async Task<IActionResult> XoaSanPham(int id)
    {
        if (!IsAdmin()) return Denied();
        try
        {
            await _sanPhamRepo.DeleteAsync(id);
            TempData["Success"] = "Đã xoá sản phẩm";
        }
        catch
        {
            TempData["Error"] = "Không thể xoá sản phẩm đã có trong đơn hàng";
        }
        return RedirectToAction(nameof(SanPham));
    }

    public async Task<IActionResult> DonHang()
    {
        if (!IsAdmin()) return Denied();
        var list = await _donHangRepo.GetAllAsync();
        return View(list);
    }

    public async Task<IActionResult> ChiTietDonHang(int id)
    {
        if (!IsAdmin()) return Denied();
        var dh = await _donHangRepo.GetDetailAsync(id);
        if (dh == null) return NotFound();
        return View("~/Views/DonHang/ChiTiet.cshtml", dh);
    }

    [HttpPost]
    public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai)
    {
        if (!IsAdmin()) return Denied();
        await _donHangRepo.UpdateTrangThaiAsync(id, trangThai);
        TempData["Success"] = "Đã cập nhật trạng thái đơn hàng";
        return RedirectToAction(nameof(DonHang));
    }

    public async Task<IActionResult> NguoiDung()
    {
        if (!IsAdmin()) return Denied();
        var list = await _nguoiDungRepo.GetAllAsync();
        return View(list);
    }
}
