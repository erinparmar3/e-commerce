using ECommerceApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /  and /Home/Index?search=&category=
    public async Task<IActionResult> Index(string? search, string? category)
    {
        var query = _context.Products.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(p => p.Category == category);
        }

        ViewBag.Categories = await _context.Products
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.SelectedCategory = category ?? "All";
        ViewBag.Search = search;

        var products = await query.OrderBy(p => p.Name).ToListAsync();
        return View(products);
    }

    // GET: /Home/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    public IActionResult Error() => View();
}
