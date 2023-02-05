using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{   
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CategoriesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public IActionResult Index()
        {

            List<Category> categories = _db.Categories.Where(x=>x.IsMain).Include(x=>x.Children).ToList();
            return View(categories);
        }
        public IActionResult Create()
        {

          ViewBag.MainCategories= _db.Categories.Where(x => x.IsMain).ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category,int? mainCatId)
        {

            ViewBag.MainCategories = _db.Categories.Where(x => x.IsMain).ToList(); if (!ModelState.IsValid)
            {
                return View();
            }

            if (category.IsMain)
            {
             
                bool isExist = await _db.Categories.AnyAsync(s => s.Name == category.Name);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category is already exist");
                    return View();
                }
                if (category.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Image can not be null");
                    return View();
                }
                if (!category.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image");
                    return View();
                }
                if (category.Photo.OlderOneMb())
                {
                    ModelState.AddModelError("Photo", "Image max 3mb");
                    return View();
                }
                string path = Path.Combine(_env.WebRootPath, "assets", "images");
                category.Image = await category.Photo.SaveFileAsync(path);

            }
            else
            {
                if(mainCatId == null)
                {
                    ModelState.AddModelError("", "Please Select Main Category");
                    return View();
                }
                Category mainCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == mainCatId);
                if(mainCategory==null)
                {
                    ModelState.AddModelError("", "Please Select Correct Main Category");
                    return View();
                }
                category.ParentId = mainCatId;
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {

            if(id==null)
            {

                return  NotFound();
            }

            Category? category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category== null)
            {

                return BadRequest();
            }
            ViewBag.MainCategories = _db.Categories.Where(x => x.IsMain).ToList();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id,Category category, int? mainCatId)
        {
            if (id == null)
            {

                return NotFound();
            }

            Category? dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (dbCategory == null)
            {

                return BadRequest();
            }

            ViewBag.MainCategories = _db.Categories.Where(x => x.IsMain).ToList(); 
            //if (!ModelState.IsValid)
            //{
            //    return View(dbCategory);
            //}

            if (dbCategory.IsMain)
            {

                bool isExist = await _db.Categories.AnyAsync(s => s.Name == category.Name);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category is already exist");
                    return View(dbCategory);
                }
                if (category.Photo != null)
                {
                    if (!category.Photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please select image");
                        return View(dbCategory);
                    }
                    if (category.Photo.OlderOneMb())
                    {
                        ModelState.AddModelError("Photo", "Image max 3mb");
                        return View(dbCategory);
                    }
                    string path = Path.Combine(_env.WebRootPath, "assets", "images");
                    dbCategory.Image = await category.Photo.SaveFileAsync(path);
                }
             

            }
            else
            {
                if (mainCatId == null)
                {
                    ModelState.AddModelError("", "Please Select Main Category");
                    return View(dbCategory);
                }
                Category mainCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == mainCatId);
                if (mainCategory == null)
                {
                    ModelState.AddModelError("", "Please Select Correct Main Category");
                    return View(dbCategory);
                }
                dbCategory.ParentId = mainCatId;
            }

            dbCategory.Name = category.Name;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}

