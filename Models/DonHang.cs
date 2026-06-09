using System.ComponentModel.DataAnnotations;

namespace StridexFinal_CSharp.Models;

public class DonHang
{
    public int Id { get; set; }
    public string MaDonHang { get; set; } = string.Empty;
    public int? NguoiDungId { get; set; }
    public DateTime NgayDat { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = "Chờ xác nhận";
    public string? TenNguoiDung { get; set; }
    public List<ChiTietDonHang> ChiTiet { get; set; } = new();
}

public class ChiTietDonHang
{
    public int Id { get; set; }
    public int? DonHangId { get; set; }
    public int? SanPhamId { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public string Hinh { get; set; } = string.Empty;
    public decimal ThanhTien => DonGia * SoLuong;
}

public class ThanhToanViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
    public string HoTen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
    public string DiaChi { get; set; } = string.Empty;

    public string GhiChu { get; set; } = string.Empty;
    public List<GioHangItem> GioHang { get; set; } = new();
    public decimal TongTien => GioHang.Sum(x => x.ThanhTien);
}
