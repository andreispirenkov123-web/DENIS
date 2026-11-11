namespace HouseholdGoodsIS.Models;

public class ProductAttribute
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class AttributeGroup
{
    public string Name { get; set; } = string.Empty;
    public List<ProductAttribute> Items { get; set; } = new();
}
