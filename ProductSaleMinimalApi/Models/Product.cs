using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProductSaleMinimalApi.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;

        [Required, Column(TypeName = "money")]
        public decimal Price { get; set; }
        public int Size { get; set; }
        public string? Picture { get; set; }
        public bool? OnSale { get; set; }
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    }
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime? Date { get; set; }
        public int? Quantity { get; set; }
        public int ProductId { get; set; }

        [JsonIgnore]
        public virtual Product? Product { get; set; }
    }

    public class UploadResponse
    {
        public string FileName { get; set; } = default!;
    }

    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Random random = new Random(42);

            DateTime baseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);


            for (int i = 1; i < 5; i++)
            {
                modelBuilder.Entity<Product>().HasData(
                    new Product
                    {
                        ProductId = i,
                        ProductName = "Product" + i,
                        Price = random.Next(1000, 2000),
                        Size = (int)random.Next(1, 5),
                        Picture = i + ".jpg",
                        OnSale = i % 2 == 0
                    });
            }
            for (int i = 1; i < 8; i++)
            {
                modelBuilder.Entity<Sale>().HasData(
                new Sale
                {
                    SaleId = i,
                    Date = baseDate.AddDays(-1 * random.Next(400, 500)),
                    ProductId = (i % 5 == 0 ? 5 : i % 5),
                    Quantity = random.Next(100, 200)
                });
            }
        }
    }
}
