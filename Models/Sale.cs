namespace HouseholdGoodsIS.Models;

/// <summary>Продажа (заказ покупателя).</summary>
public class Sale
{
    public int Id { get; set; }
    public int PickupStoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public string Status { get; set; } = "Новая";
    public string PaymentMethod { get; set; } = "Карта";
    public decimal PaidAmount { get; set; }
    public int ItemsCount { get; set; }

    public string PaymentStatus => PaidAmount >= TotalAmount ? "Оплачено" : "Не полностью";
    public string TotalAmountText => $"{TotalAmount:N0} ₽";
}
