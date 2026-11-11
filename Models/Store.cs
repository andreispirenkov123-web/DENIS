namespace HouseholdGoodsIS.Models;

/// <summary>Магазин самовывоза.</summary>
public class Store
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? WorkingHours { get; set; }

    public string DisplayName => $"{Name} — {Address}";
    public override string ToString() => DisplayName;
}
