using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Repositories;

namespace StridexFinal_CSharp.Controllers;

public class SanPhamController : Controller
{
    private readonly SanPhamRepository _repo;

    public SanPhamController(SanPhamRepository repo)
    {
        _repo = repo;
    }

    public async Task<IActionResult> Index(string? tuKhoa, string? loai, string? sort)
    {
        ViewBag.Loai = await _repo.GetLoaiAsync();
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LoaiChon = loai;
        ViewBag.Sort = sort;
        var list = await _repo.GetAllAsync(tuKhoa, loai, sort);
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sp = await _repo.GetByIdAsync(id);
        if (sp == null) return NotFound();
        return View(sp);
    }
}
