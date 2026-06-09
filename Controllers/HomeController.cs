using Microsoft.AspNetCore.Mvc;
using StridexFinal_CSharp.Repositories;

namespace StridexFinal_CSharp.Controllers;

public class HomeController : Controller
{
    private readonly SanPhamRepository _sanPhamRepo;

    public HomeController(SanPhamRepository sanPhamRepo)
    {
        _sanPhamRepo = sanPhamRepo;
    }

    public async Task<IActionResult> Index()
    {
        var sanPhamNoiBat = await _sanPhamRepo.GetNoiBatAsync(8);
        return View(sanPhamNoiBat);
    }
}
