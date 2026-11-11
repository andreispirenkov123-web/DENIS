namespace HouseholdGoodsIS.Models;

public class DashboardStats
{
    public int ProductsCount { get; set; }
    public int SalesCount { get; set; }
    public int ClientsCount { get; set; }
    public decimal Revenue { get; set; }
    public int ActiveSales { get; set; }

    public string RevenueText => $"{Revenue:N0} ₽";
}

public class StatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public double BarHeight { get; set; }
    public string CountText => Count.ToString();
}

public class CategoryTotal
{
    public string CategoryName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public int ItemsCount { get; set; }
    public decimal TotalSum { get; set; }
    public string TotalSumText => $"{TotalSum:N0} ₽";
}

public class ProductTotal
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalSum { get; set; }
    public string TotalSumText => $"{TotalSum:N0} ₽";
}

public class ClientTotal
{
    public string ClientName { get; set; } = string.Empty;
    public int SalesCount { get; set; }
    public decimal TotalSum { get; set; }
    public string TotalSumText => $"{TotalSum:N0} ₽";
}
