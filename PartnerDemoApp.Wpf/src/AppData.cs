using Microsoft.EntityFrameworkCore;

namespace PartnerDemoApp.Wpf.src;

public static class AppData
{
    public static void EnsureCreatedAndSeed()
    {
        using var db = new AppDataContext();
        db.EnsureCreatedAndSeed();
    }

    public static List<PartnerListItem> GetPartners()
    {
        using var db = new AppDataContext();

        var items = db.Partners
            .AsNoTracking()
            .Select(partner => new PartnerListItem
            {
                Id = partner.Id,
                PartnerType = partner.PartnerType!.Name,
                Name = partner.Name,
                Director = partner.Director,
                Phone = partner.Phone,
                Rating = partner.Rating,
                TotalSalesQuantity = partner.Sales.Sum(sale => (int?)sale.Quantity) ?? 0
            })
            .OrderBy(item => item.Name)
            .ToList();

        items.ForEach(item => item.DiscountPercent = PartnerDiscountCalculator.CalculateDiscountPercent(item.TotalSalesQuantity));
        return items;
    }

    public static List<AppDataContext.PartnerType> GetPartnerTypes()
    {
        using var db = new AppDataContext();
        return db.PartnerTypes.AsNoTracking().OrderBy(item => item.Name).ToList();
    }

    public static AppDataContext.Partner? GetPartnerById(int partnerId)
    {
        using var db = new AppDataContext();
        return db.Partners.AsNoTracking().FirstOrDefault(item => item.Id == partnerId);
    }

    public static void SavePartner(AppDataContext.Partner partner)
    {
        using var db = new AppDataContext();

        if (partner.Id <= 0)
        {
            db.Partners.Add(partner);
            db.SaveChanges();
            return;
        }

        var existing = db.Partners.FirstOrDefault(item => item.Id == partner.Id)
            ?? throw new InvalidOperationException("Редактируемый партнер не найден.");

        existing.PartnerTypeId = partner.PartnerTypeId;
        existing.Name = partner.Name;
        existing.Director = partner.Director;
        existing.Email = partner.Email;
        existing.Phone = partner.Phone;
        existing.LegalAddress = partner.LegalAddress;
        existing.Inn = partner.Inn;
        existing.Rating = partner.Rating;

        db.SaveChanges();
    }

    public static List<PartnerHistoryItem> GetPartnerHistory(int partnerId)
    {
        using var db = new AppDataContext();

        return db.PartnerSales
            .AsNoTracking()
            .Where(item => item.PartnerId == partnerId)
            .OrderByDescending(item => item.SaleDate)
            .Select(item => new PartnerHistoryItem
            {
                ProductName = item.Product!.Name,
                Quantity = item.Quantity,
                SaleDate = item.SaleDate
            })
            .ToList();
    }

    public static List<AppDataContext.ProductType> GetProductTypes()
    {
        using var db = new AppDataContext();
        return db.ProductTypes.AsNoTracking().OrderBy(item => item.Name).ToList();
    }

    public static List<AppDataContext.MaterialType> GetMaterialTypes()
    {
        using var db = new AppDataContext();
        return db.MaterialTypes.AsNoTracking().OrderBy(item => item.Name).ToList();
    }
}

public class PartnerListItem
{
    public int Id { get; set; }
    public string PartnerType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Rating { get; set; }
    public int TotalSalesQuantity { get; set; }
    public int DiscountPercent { get; set; }
    public string DiscountText => $"{DiscountPercent}%";
}

public class PartnerHistoryItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime SaleDate { get; set; }
    public string SaleDateText => SaleDate.ToString("dd.MM.yyyy");
}
