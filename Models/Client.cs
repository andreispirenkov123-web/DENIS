namespace HouseholdGoodsIS.Models;

/// <summary>Покупатель магазина.</summary>
public class Client
{
    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? DiscountCard { get; set; }

    public string FullName =>
        $"{LastName} {FirstName}{(string.IsNullOrWhiteSpace(MiddleName) ? string.Empty : " " + MiddleName)}";
}
