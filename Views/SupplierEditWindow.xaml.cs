using System.Windows;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class SupplierEditWindow : Window
{
    private readonly Supplier _supplier;

    public SupplierEditWindow(Supplier? supplier)
    {
        InitializeComponent();
        _supplier = supplier ?? new Supplier();
        TitleText.Text = supplier == null ? "Добавление поставщика" : "Редактирование поставщика";
        if (supplier == null) return;
        NameBox.Text = _supplier.Name;
        InnBox.Text = _supplier.Inn ?? string.Empty;
        CityBox.Text = _supplier.City ?? string.Empty;
        ContactBox.Text = _supplier.ContactPerson ?? string.Empty;
        PhoneBox.Text = _supplier.Phone ?? string.Empty;
        EmailBox.Text = _supplier.Email ?? string.Empty;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            ErrorText.Text = "Укажите название организации.";
            return;
        }
        _supplier.Name = NameBox.Text.Trim();
        _supplier.Inn = string.IsNullOrWhiteSpace(InnBox.Text) ? null : InnBox.Text.Trim();
        _supplier.City = string.IsNullOrWhiteSpace(CityBox.Text) ? null : CityBox.Text.Trim();
        _supplier.ContactPerson = string.IsNullOrWhiteSpace(ContactBox.Text) ? null : ContactBox.Text.Trim();
        _supplier.Phone = string.IsNullOrWhiteSpace(PhoneBox.Text) ? null : PhoneBox.Text.Trim();
        _supplier.Email = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text.Trim();
        try
        {
            Database.SaveSupplier(_supplier);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
