using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace PartnerDemoApp.Wpf.src;

public class AppDataContext : DbContext
{
    private static readonly string ArtifactsFolder = Path.Combine(AppContext.BaseDirectory, "artifacts");
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public DbSet<MaterialType> MaterialTypes => Set<MaterialType>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PartnerType> PartnerTypes => Set<PartnerType>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<PartnerSale> PartnerSales => Set<PartnerSale>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }

    public void EnsureCreatedAndSeed()
    {
        Directory.CreateDirectory(ArtifactsFolder);
        Database.EnsureCreated();

        try
        {
            if (Partners.Any() || Products.Any() || PartnerSales.Any())
            {
                GenerateScript();
                return;
            }
        }
        catch
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        var partnerImports = LoadXlsx.LoadExcel<LoadXlsx.PartnersImport>("Partners_import.xlsx");
        var productImports = LoadXlsx.LoadExcel<LoadXlsx.ProductsImport>("Products_import.xlsx");
        var productTypeImports = LoadXlsx.LoadExcel<LoadXlsx.ProductTypeImport>("Product_type_import.xlsx");
        var materialTypeImports = LoadXlsx.LoadExcel<LoadXlsx.MaterialTypeImport>("Material_type_import.xlsx");
        var partnerProductImports = LoadXlsx.LoadExcel<LoadXlsx.PartnerProductsImport>("Partner_products_import.xlsx");

        var materialTypes = materialTypeImports
            .Select(item => new MaterialType { Name = item.Type.Trim(), DefectPercent = item.Ratio })
            .ToList();

        var productTypes = productTypeImports
            .Select(item => new ProductType { Name = item.Type.Trim(), Coefficient = item.Ratio })
            .ToList();

        var partnerTypes = partnerImports
            .Select(item => item.PartnerType.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new PartnerType { Name = name })
            .ToList();

        var productTypeByName = productTypes.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
        var partnerTypeByName = partnerTypes.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        var products = productImports
            .Select(item => new Product
            {
                Article = item.Article.Trim(),
                ProductType = productTypeByName[item.Type.Trim()],
                Name = item.Name.Trim(),
                MinPartnerPrice = (decimal)item.MinPrice
            })
            .ToList();

        var partners = partnerImports
            .Select(item => new Partner
            {
                PartnerType = partnerTypeByName[item.PartnerType.Trim()],
                Name = item.Name.Trim(),
                Director = item.DirectorName.Trim(),
                Email = item.Email.Trim(),
                Phone = item.PhoneNumber.Trim(),
                LegalAddress = item.Adress.Trim(),
                Inn = item.Inn.Trim(),
                Rating = item.Rate
            })
            .ToList();

        var productByName = products.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
        var partnerByName = partners.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        var partnerSales = partnerProductImports
            .Select(item => new PartnerSale
            {
                Product = productByName[item.Product.Trim()],
                Partner = partnerByName[item.PartnerName.Trim()],
                Quantity = item.Count,
                SaleDate = ParseDate(item.Date)
            })
            .ToList();

        MaterialTypes.AddRange(materialTypes);
        ProductTypes.AddRange(productTypes);
        PartnerTypes.AddRange(partnerTypes);
        Products.AddRange(products);
        Partners.AddRange(partners);
        PartnerSales.AddRange(partnerSales);

        SaveChanges();
        GenerateScript();
    }

    private static DateTime ParseDate(string value)
    {
        if (DateTime.TryParse(value, RuCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        return DateTime.Today;
    }

    private void GenerateScript()
    {
        var scriptPath = Path.Combine(ArtifactsFolder, "database_script.sql");
        var script = Database.GenerateCreateScript();
        File.WriteAllText(scriptPath, script);
    }

    public class MaterialType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double DefectPercent { get; set; }
    }

    public class ProductType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public double Coefficient { get; set; }
        public List<Product> Products { get; set; } = new();
    }

    public class Product
    {
        public int Id { get; set; }
        public int ProductTypeId { get; set; }
        public ProductType? ProductType { get; set; }
        public required string Name { get; set; }
        public required string Article { get; set; }
        public decimal MinPartnerPrice { get; set; }
        public List<PartnerSale> Sales { get; set; } = new();
    }

    public class PartnerType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Partner> Partners { get; set; } = new();
    }

    public class Partner
    {
        public int Id { get; set; }
        public int PartnerTypeId { get; set; }
        public PartnerType? PartnerType { get; set; }
        public required string Name { get; set; }
        public required string Director { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string LegalAddress { get; set; }
        public required string Inn { get; set; }
        public int Rating { get; set; }
        public List<PartnerSale> Sales { get; set; } = new();
    }

    public class PartnerSale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int PartnerId { get; set; }
        public Partner? Partner { get; set; }
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
