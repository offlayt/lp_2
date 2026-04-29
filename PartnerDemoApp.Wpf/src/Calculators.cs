using Microsoft.EntityFrameworkCore;

namespace PartnerDemoApp.Wpf.src;

public static class PartnerDiscountCalculator
{
    public static int CalculateDiscountPercent(int totalSalesQuantity)
    {
        if (totalSalesQuantity < 10000) return 0;
        if (totalSalesQuantity < 50000) return 5;
        if (totalSalesQuantity < 300000) return 10;
        return 15;
    }
}

public static class MaterialCalculator
{
    public static int CalculateRequiredMaterial(
        int productTypeId,
        int materialTypeId,
        int quantity,
        double parameter1,
        double parameter2)
    {
        if (productTypeId <= 0 || materialTypeId <= 0 || quantity <= 0) return -1;
        if (parameter1 <= 0 || parameter2 <= 0) return -1;

        using var db = new AppDataContext();

        var productType = db.ProductTypes.AsNoTracking().FirstOrDefault(item => item.Id == productTypeId);
        var materialType = db.MaterialTypes.AsNoTracking().FirstOrDefault(item => item.Id == materialTypeId);

        if (productType is null || materialType is null) return -1;

        var amountWithoutDefect = quantity * parameter1 * parameter2 * productType.Coefficient;
        var amountWithDefect = amountWithoutDefect * (1 + materialType.DefectPercent);

        return (int)Math.Ceiling(amountWithDefect);
    }
}
