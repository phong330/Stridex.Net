using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Extensions;
using StridexFinal_CSharp.Models;
using StridexFinal_CSharp.Repositories;

namespace StridexFinal_CSharp.Controllers;

public class GioHangController : Controller
{
    private const string CartKey = "GioHang";
    private readonly SanPhamRepository _sanPhamRepo;

    public GioHangController(SanPhamRepository sanPhamRepo)
    {
        _sanPhamRepo = sanPhamRepo;
    }

    public IActionResult Index()
    {
        var gioHang = LayGioHang();
        return View(gioHang);
    }

    public async Task<IActionResult> Them(int id, int soLuong = 1)
    {
        var sp = await _sanPhamRepo.GetByIdAsync(id);
        if (sp == null) return NotFound();
        soLuong = Math.Max(1, soLuong);

        var gioHang = LayGioHang();
        var item = gioHang.FirstOrDefault(x => x.SanPhamId == id);
        if (item == null)
        {
            gioHang.Add(new GioHangItem
            {
                SanPhamId = sp.Id,
                Ten = sp.Ten,
                Gia = sp.Gia,
                Hinh = sp.Hinh,
                SoLuong = soLuong
            });
        }
        else
        {
            item.SoLuong += soLuong;
        }

        LuuGioHang(gioHang);
        TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CapNhat(int sanPhamId, int soLuong)
    {
        var gioHang = LayGioHang();
        var item = gioHang.FirstOrDefault(x => x.SanPhamId == sanPhamId);
        if (item != null)
        {
            if (soLuong <= 0) gioHang.Remove(item);
            else item.SoLuong = soLuong;
        }
        LuuGioHang(gioHang);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Xoa(int id)
    {
        var gioHang = LayGioHang();
        gioHang.RemoveAll(x => x.SanPhamId == id);
        LuuGioHang(gioHang);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult XoaTatCa()
    {
        HttpContext.Session.Remove(CartKey);
        return RedirectToAction(nameof(Index));
    }

    private List<GioHangItem> LayGioHang() => HttpContext.Session.GetObject<List<GioHangItem>>(CartKey) ?? new();
    private void LuuGioHang(List<GioHangItem> gioHang) => HttpContext.Session.SetObject(CartKey, gioHang);
}
