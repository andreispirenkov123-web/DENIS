namespace HouseholdGoodsIS.Models;

/// <summary>Сотрудник магазина — пользователь системы.</summary>
public class AppUser
{
    public int Id { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? LastLogin { get; set; }

    public string FullName =>
        $"{LastName} {FirstName}{(string.IsNullOrWhiteSpace(MiddleName) ? string.Empty : " " + MiddleName)}";
    public string BlockedText => IsBlocked ? "Да" : "Нет";
    public string LastLoginText => LastLogin?.ToString("dd.MM.yyyy HH:mm") ?? "—";
}
