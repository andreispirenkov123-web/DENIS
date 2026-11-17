namespace HouseholdGoodsIS.Models;

public class SupplyItem
{
    public int Id { get; set; }
    public int SupplyId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal TotalPrice { get; set; }

    public string PurchasePriceText => $"{PurchasePrice:N0} ₽";
    public string TotalPriceText => $"{TotalPrice:N0} ₽";
}
