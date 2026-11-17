namespace HouseholdGoodsIS.Models;

public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public string UnitPriceText => $"{UnitPrice:N0} ₽";
    public string TotalPriceText => $"{TotalPrice:N0} ₽";
}
