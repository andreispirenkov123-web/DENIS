namespace HouseholdGoodsIS.Models;

public class Payment
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Карта";
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public string? Purpose { get; set; }

    public string AmountText => $"{Amount:N0} ₽";
}
