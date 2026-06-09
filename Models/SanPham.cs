using System.ComponentModel.DataAnnotations;

namespace StridexFinal_CSharp.Models;

public class SanPham
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
    [Display(Name = "Tên sản phẩm")]
    public string Ten { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập loại sản phẩm")]
    [Display(Name = "Loại")]
    public string Loai { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập giá")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá không hợp lệ")]
    [Display(Name = "Giá")]
    public decimal Gia { get; set; }

    [Display(Name = "Hình ảnh")]
    public string Hinh { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    public string MoTa { get; set; } = string.Empty;

    [Display(Name = "Nổi bật")]
    public bool NoiBat { get; set; }
}
