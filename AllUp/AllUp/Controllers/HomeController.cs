using AllUp.DAL;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AllUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db)
        {
            _db = db;
        }
    
        public IActionResult Index()
        {
            List<Category> categories = _db.Categories.Where(x=>x.IsMain).ToList();
            return View(categories);
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}