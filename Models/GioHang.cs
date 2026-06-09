namespace StridexFinal_CSharp.Models;

public class GioHangItem
{
    public int SanPhamId { get; set; }
    public string Ten { get; set; } = string.Empty;
    public decimal Gia { get; set; }
    public string Hinh { get; set; } = string.Empty;
    public int SoLuong { get; set; }
    public decimal ThanhTien => Gia * SoLuong;
}
