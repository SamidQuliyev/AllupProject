using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public int Rate { get; set; }
        [NotMapped]
        public List<IFormFile>?Photos { get; set; }
        public ProductDetail ProductDetail { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public List<ProductImage> ProductImages { get; set; }
      

    }
}
