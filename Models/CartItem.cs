using System.Globalization;

namespace HouseholdGoodsIS.Models;

public class CartItem
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

    public Product Product { get; set; } = new();
    public int Quantity { get; set; } = 1;

    public int ProductId => Product.Id;
    public string Name => Product.Name;
    public string Sku => Product.Sku;
    public string? ImagePath => Product.ImagePath;
    public string PriceText => Product.PriceText;
    public decimal Sum => Product.RetailPrice * Quantity;
    public string SumText => $"{Sum.ToString("N0", Ru)} ₽";
    public int Stock => Product.Stock;
}
