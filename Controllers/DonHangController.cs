using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Extensions;
using StridexFinal_CSharp.Models;
using StridexFinal_CSharp.Repositories;
using StridexFinal_CSharp.Services;

namespace StridexFinal_CSharp.Controllers;

public class DonHangController : Controller
{
    private const string CartKey = "GioHang";
    private readonly DonHangRepository _repo;
    private readonly VnPayService _vnPayService;

    public DonHangController(
        DonHangRepository repo,
        VnPayService vnPayService)
    {
        _repo = repo;
        _vnPayService = vnPayService;
    }

    [HttpGet]
    public IActionResult ThanhToan()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction(
                "DangNhap",
                "TaiKhoan",
                new { returnUrl = Url.Action("ThanhToan", "DonHang") }
            );
        }

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
    public async Task<IActionResult> ThanhToan(
        ThanhToanViewModel model,
        string action)
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var gioHang = LayGioHang();
        model.GioHang = gioHang;

        if (!gioHang.Any())
        {
            ModelState.AddModelError(string.Empty, "Giỏ hàng đang rỗng");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var donHangId = await _repo.TaoDonHangAsync(
            userId.Value,
            gioHang
        );

        // Đặt hàng thường
        if (action == "COD")
        {
            HttpContext.Session.Remove(CartKey);

            TempData["Success"] =
                "Đặt hàng thành công";

            return RedirectToAction(nameof(LichSu));
        }

        // Thanh toán VNPay
        var tongTien = gioHang.Sum(x => x.ThanhTien);

        HttpContext.Session.Remove(CartKey);

        var paymentUrl = _vnPayService.CreatePaymentUrl(
            HttpContext,
            donHangId,
            tongTien
        );

        return Redirect(paymentUrl);
    }

    public IActionResult VnPayReturn()
    {
        var isValid = _vnPayService.ValidateSignature(Request.Query);

        if (!isValid)
        {
            TempData["Error"] = "Chữ ký VNPay không hợp lệ";
            return RedirectToAction("Index", "Home");
        }

        var responseCode = Request.Query["vnp_ResponseCode"].ToString();
        var orderIdText = Request.Query["vnp_TxnRef"].ToString();

        if (!int.TryParse(orderIdText, out var orderId))
        {
            TempData["Error"] = "Không tìm thấy mã đơn hàng";
            return RedirectToAction("Index", "Home");
        }

        if (responseCode == "00")
        {
            TempData["Success"] = "Thanh toán VNPay thành công";
        }
        else
        {
            TempData["Error"] = "Thanh toán VNPay thất bại hoặc đã bị hủy";
        }

        return RedirectToAction(nameof(ChiTiet), new { id = orderId });
    }

    public async Task<IActionResult> LichSu()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var list = await _repo.GetByNguoiDungAsync(userId.Value);
        return View(list);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var donHang = await _repo.GetDetailAsync(id);

        if (donHang == null)
        {
            return NotFound();
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var vaiTro = HttpContext.Session.GetString("VaiTro");

        if (!string.Equals(vaiTro, "Admin", StringComparison.OrdinalIgnoreCase)
            && donHang.NguoiDungId != userId)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        return View(donHang);
    }

    private List<GioHangItem> LayGioHang()
    {
        return HttpContext.Session.GetObject<List<GioHangItem>>(CartKey) ?? new();
    }
}
