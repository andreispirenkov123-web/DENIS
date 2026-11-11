namespace HouseholdGoodsIS.Models;

/// <summary>Бытовой товар (техника).</summary>
public class Product
{
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
    public int WarrantyMonths { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; }
    public int Stock { get; set; }

    public string PriceText => $"{RetailPrice:N0} ₽";
    public string PurchasePriceText => $"{PurchasePrice:N0} ₽";
    public string StockText => $"Остаток: {Stock} шт.";
    public string WarrantyText => $"Гарантия {WarrantyMonths} мес.";
    public string ActiveText => IsActive ? "В продаже" : "Снят с продажи";
    public override string ToString() => $"{Name} ({Sku})";
}
