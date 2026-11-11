namespace HouseholdGoodsIS.Models;

/// <summary>Категория бытовых товаров.</summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public string ProductCountText => $"{ProductCount} товаров";
    public override string ToString() => Name;
}
