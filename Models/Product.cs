using System.Globalization;

namespace HouseholdGoodsIS.Models;

/// <summary>Бытовой товар (техника).</summary>
public class Product
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal? OldPrice { get; set; }
    public decimal Rating { get; set; }
    public int ReviewsCount { get; set; }
    public int WarrantyMonths { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; }
    public int Stock { get; set; }

    public string PriceText => $"{RetailPrice.ToString("N0", Ru)} ₽";
    public string PurchasePriceText => $"{PurchasePrice.ToString("N0", Ru)} ₽";
    public string OldPriceText => OldPrice.HasValue ? $"{OldPrice.Value.ToString("N0", Ru)} ₽" : string.Empty;
    public bool HasOldPrice => OldPrice.HasValue && OldPrice.Value > RetailPrice;
    public string DiscountText => HasOldPrice
        ? $"-{Math.Round((1 - RetailPrice / OldPrice!.Value) * 100):0}%"
        : string.Empty;
    public string RatingText => Rating.ToString("0.0", Ru);
    public string ReviewsText => $"({ReviewsCount})";
    public string StockText => $"Остаток: {Stock} шт.";
    public string WarrantyText => $"Гарантия {WarrantyMonths} мес.";
    public string ActiveText => IsActive ? "В продаже" : "Снят с продажи";
    public override string ToString() => $"{Name} ({Sku})";
}
