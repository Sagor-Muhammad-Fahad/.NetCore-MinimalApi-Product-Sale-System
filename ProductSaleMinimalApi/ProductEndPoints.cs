using Microsoft.EntityFrameworkCore;
using ProductSaleMinimalApi.Models;
using System.Formats.Asn1;
using System.Reflection.Metadata.Ecma335;

namespace ProductSaleMinimalApi
{
    public static class ProductEndPoints
    {
        public static void MapProductSaleEndPoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/Products").WithTags("Products");
            group.MapGet("/", async (ProductDbContext db) =>
            {
                var products = await db.Products.ToListAsync();
                return TypedResults.Ok(products);
            }).WithName("GetProducts");

            group.MapGet("/Sale/Include", async (ProductDbContext db) =>
            {
                var products = await db.Products.Include(p => p.Sales).ToListAsync();
                return TypedResults.Ok(products);
            }).WithName("GetProductsWithSale");

            group.MapGet("/{id:int}", async (int id, ProductDbContext db) =>
            {
                var product = await db.Products.FindAsync(id);
                return product is not null ? TypedResults.Ok(product) : Results.NotFound();
            }).WithName("GetProduct");

            group.MapGet("{id:int}/Include", async (int id, ProductDbContext db) =>
            {
                var product = await db.Products.Include(p => p.Sales).FirstOrDefaultAsync(p => p.ProductId == id);
                return product is not null ? TypedResults.Ok(product) : Results.NotFound();
            }).WithName("GetProductWithSale");

            group.MapPost("/",async (Product product, ProductDbContext db) =>
            {
                if (string.IsNullOrEmpty(product.Picture))
                {
                    product.Picture = "noimage.png";
                }
                product.Sales ??= new List<Sale>();
                db.Products.Add(product);
                await db.SaveChangesAsync();
                foreach (var sale in product.Sales)
                {
                    sale.Product = null;
                }
                return TypedResults.CreatedAtRoute(product, "GetProduct", new { id = product.ProductId });
               
            }).WithName("SaveProduct");

            group.MapPost("/Upload/{id:int}", async (int id, IFormFile file, ProductDbContext db, IWebHostEnvironment env) =>
            {
                var product = await db.Products.FindAsync(id);
                if (product is null)
                {
                    return Results.NotFound();
                }
                if (file is null || file.Length == 0) return Results.BadRequest("No file Upload.");
                if (file.ContentType != "image/jpeg" && file.ContentType != "image/png")
                {
                    return Results.BadRequest("Invalid file type.only Jpeg and png are allowed.");
                }
                if (file.Length > 2 * 1024 * 1024)
                {
                    return Results.BadRequest("File size exeeds the limit of 2mb");
                }
                string ext = Path.GetExtension(file.FileName);
                string fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ext;
                var imagesFolder = Path.Combine(env.EnvironmentName, "images");
                var savePath = Path.Combine(imagesFolder, fileName);
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }
                try
                {
                    using (var fileStream = new FileStream(savePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    product.Picture = fileName;
                    await db.SaveChangesAsync();
                    return TypedResults.Ok(new UploadResponse { FileName = product.Picture });
                }
                catch (IOException ex)
                {
                    return Results.Problem($"Error saving file: {ex.Message}", statusCode: 500);
                   
                }

            }).DisableAntiforgery().WithName("UploadProductPicture");

            group.MapPut("/{id:int}", async (int id, Product product, ProductDbContext db) =>
            {
                if (id != product.ProductId) return Results.BadRequest();
                try
                {
                    var existingProduct = await db.Products.Include(p => p.Sales).FirstOrDefaultAsync(p => p.ProductId == id);
                    if (existingProduct == null) return Results.NotFound();
                    db.Entry(existingProduct).CurrentValues.SetValues(product);
                    if (product.Sales != null)
                    {
                        var salesToRemove = existingProduct.Sales.Where(existingSale => !product.Sales.Any(updateSale => updateSale.SaleId == existingSale.SaleId)).ToList();
                        db.Sales.RemoveRange(salesToRemove);
                        foreach (var updateSale in product.Sales)
                        {
                            var existingSale = existingProduct.Sales.FirstOrDefault(s => s.SaleId == updateSale.SaleId);
                            if (existingSale != null)
                            {
                                db.Entry(existingSale).CurrentValues.SetValues(updateSale);
                                db.Entry(existingSale).State = EntityState.Modified;
                            }
                            else
                            {
                                existingProduct.Sales.Add(updateSale);
                            }
                        }
                        await db.SaveChangesAsync();
                        foreach (var sale in existingProduct.Sales)
                        {
                            sale.Product = null;
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool exits = await db.Products.AnyAsync(p => p.ProductId == id);
                    if (!exits) return Results.NotFound();
                    throw;
                }
                return TypedResults.NoContent();
            }).WithName("UpdateProduct");

            group.MapDelete("/{id:int}", async (int id, ProductDbContext db) =>
            {
                var product = await db.Products.FindAsync(id);
                if (product == null) return Results.NotFound();
                db.Products.Remove(product);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }).WithName("DeleteProduct");
        }
    }
}
