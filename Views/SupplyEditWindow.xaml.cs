using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class SupplyEditWindow : Window
{
    private readonly Supply _supply;
    private readonly ObservableCollection<SupplyItem> _items = new();

    public SupplyEditWindow(Supply? supply)
    {
        InitializeComponent();
        _supply = supply ?? new Supply();
        TitleText.Text = supply == null ? "Оформление поставки" : $"Поставка {_supply.InvoiceNumber}";

        SupplierBox.ItemsSource = Database.GetSuppliers();
        ManagerBox.ItemsSource = Database.GetUsers().Where(u => u.RoleId is 1 or 2).ToList();
        SupplyDateBox.SelectedDate = DateTime.Today;
        StatusBox.SelectedIndex = 0;
        ItemsGrid.ItemsSource = _items;

        if (supply == null) return;

        SupplierBox.SelectedItem = ((List<Supplier>)SupplierBox.ItemsSource).FirstOrDefault(s => s.Id == _supply.SupplierId);
        ManagerBox.SelectedItem = ((List<AppUser>)ManagerBox.ItemsSource).FirstOrDefault(u => u.Id == _supply.ManagerId);
        InvoiceBox.Text = _supply.InvoiceNumber;
        SupplyDateBox.SelectedDate = _supply.SupplyDate;
        foreach (var item in StatusBox.Items)
            if (item is ComboBoxItem ci && ci.Content?.ToString() == _supply.Status)
            {
                StatusBox.SelectedItem = ci;
                break;
            }
        foreach (var item in Database.GetSupplyItems(_supply.Id)) _items.Add(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        if (TotalText == null) return;
        var total = _items.Sum(i => i.TotalPrice);
        TotalText.Text = $"{total:N0} ₽";
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ItemEditWindow(false) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.SupplyItem == null) return;
        _items.Add(dialog.SupplyItem);
        RecalculateTotal();
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsGrid.SelectedItem is not SupplyItem item) return;
        _items.Remove(item);
        RecalculateTotal();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (SupplierBox.SelectedItem is not Supplier supplier || ManagerBox.SelectedItem is not AppUser manager)
        {
            ErrorText.Text = "Выберите поставщика и менеджера.";
            return;
        }
        if (string.IsNullOrWhiteSpace(InvoiceBox.Text))
        {
            ErrorText.Text = "Укажите номер накладной.";
            return;
        }
        if (_items.Count == 0)
        {
            ErrorText.Text = "Добавьте хотя бы один товар.";
            return;
        }
        _supply.SupplierId = supplier.Id;
        _supply.ManagerId = manager.Id;
        _supply.InvoiceNumber = InvoiceBox.Text.Trim();
        _supply.SupplyDate = SupplyDateBox.SelectedDate ?? DateTime.Today;
        _supply.Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Оформлена";
        try
        {
            Database.SaveSupply(_supply, _items.ToList());
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить поставку: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

