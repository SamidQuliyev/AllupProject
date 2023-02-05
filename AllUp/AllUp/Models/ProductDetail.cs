using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp.Models
{
    public class ProductDetail
    {
        public int Id { get; set; }

        public string? Tax { get; set; }

        public string? Tags { get; set; }

        public string? Brand { get; set; }

        public string? Description { get; set; }

        public bool HasStock { get; set; }
        [ForeignKey("Product")]

        public int ProductId { get; set; }

        public Product? Product { get; set; }

    }
}
