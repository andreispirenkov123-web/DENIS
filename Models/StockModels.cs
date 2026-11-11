using System.Globalization;

namespace HouseholdGoodsIS.Models;

public class StockSummary
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public int ProductsCount { get; set; }
    public int UnitsCount { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal RetailValue { get; set; }
    public int LowStockCount { get; set; }

    public string PurchaseValueText => $"{PurchaseValue.ToString("N0", Ru)} ₽";
    public string RetailValueText => $"{RetailValue.ToString("N0", Ru)} ₽";
    public string LowStockText => LowStockCount.ToString();
}

public class StockMovement
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public string MovementType { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public string Document { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime MovementDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Comment { get; set; }

    public bool IsIncome => Quantity > 0;
    public string QuantityText => (Quantity > 0 ? "+" : "−") + Math.Abs(Quantity);
    public string DateText => MovementDate.ToString("dd.MM.yyyy", Ru);
}
