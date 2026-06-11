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

    public async Task<IActionResult> Index(
        string? tuKhoa,
        string? loai,
        string? sort,
        int page = 1)
    {
        int pageSize = 20;

        ViewBag.Loai = await _repo.GetLoaiAsync();
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.LoaiChon = loai;
        ViewBag.Sort = sort;

        var list = await _repo.GetAllAsync(tuKhoa, loai, sort);

        int totalItems = list.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var pagedList = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(pagedList);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sp = await _repo.GetByIdAsync(id);
        if (sp == null) return NotFound();
        return View(sp);
    }
}