using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public IActionResult Index()
        {
            List<Product> products = _db.Products.Include(x => x.ProductImages).Include(x => x.ProductDetail).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).ToList();
            return View(products);
        }
        #region Create
        public async Task<IActionResult> Create()
        {

            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            Category? category = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.IsMain);
            ViewBag.ChildCategories = category.Children;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int? mainCatId, int? childCatId)
        {

            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            Category? category = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.IsMain);
            ViewBag.ChildCategories = category.Children;

            if (product.Photos == null)
            {
                ModelState.AddModelError("Photos", "Images can not be null");
                return View();
            }
            if (mainCatId == null)
            {
                ModelState.AddModelError("", "main cat can not be null");
                return View();

            }

            List<ProductImage> productImages = new List<ProductImage>();

            foreach (IFormFile Photo in product.Photos)
            {
                ProductImage productImage = new ProductImage();

                if (!Photo.IsImage())
                {
                    ModelState.AddModelError("Photos", "Please select image");
                    return View();
                }
                if (Photo.OlderOneMb())
                {
                    ModelState.AddModelError("Photos", "Image max 3mb");
                    return View();
                }
                string path = Path.Combine(_env.WebRootPath, "assets", "images", "product");
                productImage.Image = await Photo.SaveFileAsync(path);
                productImages.Add(productImage);

            }
            List<ProductCategory> productCategories = new List<ProductCategory>();
            ProductCategory productMainCategory = new ProductCategory();
            productMainCategory.CategoryId = (int)mainCatId;
            productCategories.Add(productMainCategory);

            if (childCatId != null)
            {

                ProductCategory productChildCategory = new ProductCategory();
                productChildCategory.CategoryId = (int)childCatId;
                productCategories.Add(productChildCategory);
            }


            product.ProductImages = productImages;
            product.ProductCategories = productCategories;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();


            return RedirectToAction("Index");
        }
        #endregion


        #region Update
        #region Get
        public async Task<IActionResult> Update(int? id)
        {

            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            Category? category = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.IsMain);

            if (id == null)
            {
                return NotFound();
            }
            Product? product = await _db.Products.Include(x => x.ProductImages).Include(x => x.ProductDetail).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).
                ThenInclude(x => x.Children).
                FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return BadRequest();
            }
            return View(product);
        }
        #endregion

        #region Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product, int? mainCatId, int? childCatId)
        {

            #region From Get
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            Category? category = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.IsMain);

            if (id == null)
            {
                return NotFound();
            }
            Product? dbProduct = await _db.Products.Include(x => x.ProductImages).Include(x => x.ProductDetail).
                Include(x => x.ProductCategories).
                ThenInclude(x => x.Category).
                ThenInclude(x => x.Children).
                FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }


            #endregion
         
            if (mainCatId == null)
            {
                ModelState.AddModelError("", "main cat can not be null");
                return View();

            }

          
            if (product.Photos != null)
            {
                List<ProductImage> productImages = new List<ProductImage>();
                foreach (IFormFile Photo in product.Photos)
                {
                    ProductImage productImage = new ProductImage();

                    if (!Photo.IsImage())
                    {
                        ModelState.AddModelError("Photos", "Please select image");
                        return View();
                    }
                    if (Photo.OlderOneMb())
                    {
                        ModelState.AddModelError("Photos", "Image max 3mb");
                        return View();
                    }
                    string path = Path.Combine(_env.WebRootPath, "assets", "images", "product");
                    productImage.Image = await Photo.SaveFileAsync(path);
                    productImages.Add(productImage);

                }

                dbProduct.ProductImages.AddRange(productImages);
            }
    
            List<ProductCategory> productCategories = new List<ProductCategory>();
            ProductCategory productMainCategory = new ProductCategory();
            productMainCategory.CategoryId = (int)mainCatId;
            productCategories.Add(productMainCategory);

            if (childCatId != null)
            {

                ProductCategory productChildCategory = new ProductCategory();
                productChildCategory.CategoryId = (int)childCatId;
                productCategories.Add(productChildCategory);
            }


            dbProduct.ProductCategories = productCategories;
            dbProduct.Name = product.Name;
            dbProduct.ProductDetail.Tags = product.ProductDetail.Tags;
            await _db.SaveChangesAsync();
            return RedirectToAction("");
        }

        #endregion

        #endregion

        #region LoadChild
        public async Task<IActionResult> LoadChildCategories(int mainId)
        {
            List<Category> categories = await _db.Categories.Where(x => x.ParentId == mainId).ToListAsync();
            return PartialView("_ChildCategoriesPartial", categories);
        }
        #endregion

        #region DeleteImages
        public async Task<IActionResult> DeleteImages(int? imgId)
        {
            if (imgId == null)
            {

                return NotFound();
            }

            ProductImage productImage = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imgId);
            if (productImage == null)
            {
                return BadRequest();

            }
            int count = _db.ProductImages.Count(x=>x.ProductId == productImage.ProductId);

            if (count > 1) { 
           

                string path = Path.Combine(_env.WebRootPath, "assets", "images", "product", productImage.Image);
                if (System.IO.File.Exists(path))
                {

                    System.IO.File.Delete(path);
                }

                _db.ProductImages.Remove(productImage);
                await _db.SaveChangesAsync();
                return Ok();
            }
            else
            {

                return BadRequest();
            }

        }
        #endregion



    }
}
