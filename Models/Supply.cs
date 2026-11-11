namespace HouseholdGoodsIS.Models;

/// <summary>Поставка товаров от поставщика.</summary>
public class Supply
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SupplyDate { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = "Оформлена";
    public int ItemsCount { get; set; }

    public string TotalCostText => $"{TotalCost:N0} ₽";
}
