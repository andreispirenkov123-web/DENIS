namespace HouseholdGoodsIS.Models;

public class StoreTotal
{
    public string StoreName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal TotalSum { get; set; }
    public string TotalSumText => $"{TotalSum:N0} ₽";
}
