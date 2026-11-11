using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class SaleEditWindow : Window
{
    private readonly Sale _sale;
    private readonly ObservableCollection<SaleItem> _items = new();
    private readonly List<Payment> _payments = new();

    public SaleEditWindow(Sale? sale)
    {
        InitializeComponent();
        _sale = sale ?? new Sale();
        TitleText.Text = sale == null ? "Оформление продажи" : $"Продажа №{_sale.Id}";

        ClientBox.ItemsSource = Database.GetClients();
        ManagerBox.ItemsSource = Database.GetUsers().Where(u => u.RoleId is 1 or 2).ToList();
        SaleDateBox.SelectedDate = DateTime.Today;
        StatusBox.SelectedIndex = 0;
        MethodBox.SelectedIndex = 0;
        ItemsGrid.ItemsSource = _items;

        if (sale == null) return;

        ClientBox.SelectedItem = ((List<Client>)ClientBox.ItemsSource).FirstOrDefault(c => c.Id == _sale.ClientId);
        ManagerBox.SelectedItem = ((List<AppUser>)ManagerBox.ItemsSource).FirstOrDefault(u => u.Id == _sale.ManagerId);
        SaleDateBox.SelectedDate = _sale.SaleDate;
        DiscountBox.Text = _sale.Discount.ToString("0.00");
        foreach (var item in StatusBox.Items)
            if (item is ComboBoxItem ci && ci.Content?.ToString() == _sale.Status)
            {
                StatusBox.SelectedItem = ci;
                break;
            }
        foreach (var item in MethodBox.Items)
            if (item is ComboBoxItem ci && ci.Content?.ToString() == _sale.PaymentMethod)
            {
                MethodBox.SelectedItem = ci;
                break;
            }
        foreach (var item in Database.GetSaleItems(_sale.Id)) _items.Add(item);
        _payments.AddRange(Database.GetPayments(_sale.Id));
        PaymentsGrid.ItemsSource = _payments;
        RecalculateTotal();
    }

    private decimal DiscountValue => decimal.TryParse(DiscountBox.Text.Replace(',', '.'), NumberStyles.Any,
        CultureInfo.InvariantCulture, out var value) ? value : 0;

    private void RecalculateTotal()
    {
        if (TotalText == null) return;
        var total = _items.Sum(i => i.TotalPrice) - DiscountValue;
        if (total < 0) total = 0;
        TotalText.Text = $"{total:N0} ₽";
    }

    private void DiscountBox_TextChanged(object sender, TextChangedEventArgs e) => RecalculateTotal();

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ItemEditWindow(true) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.SaleItem == null) return;
        _items.Add(dialog.SaleItem);
        RecalculateTotal();
    }

    private void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsGrid.SelectedItem is not SaleItem item) return;
        _items.Remove(item);
        RecalculateTotal();
    }

    private void AddPayment_Click(object sender, RoutedEventArgs e)
    {
        if (_sale.Id == 0)
        {
            ErrorText.Text = "Сначала сохраните продажу, затем добавляйте платежи.";
            return;
        }
        var dialog = new PaymentEditWindow(_sale.Id, null) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        _payments.Clear();
        _payments.AddRange(Database.GetPayments(_sale.Id));
        PaymentsGrid.ItemsSource = null;
        PaymentsGrid.ItemsSource = _payments;
    }

    private void DeletePayment_Click(object sender, RoutedEventArgs e)
    {
        if (PaymentsGrid.SelectedItem is not Payment payment) return;
        var result = MessageBox.Show("Удалить выбранный платёж?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            Database.DeletePayment(payment.Id);
            _payments.Clear();
            _payments.AddRange(Database.GetPayments(_sale.Id));
            PaymentsGrid.ItemsSource = null;
            PaymentsGrid.ItemsSource = _payments;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось удалить платёж: " + ex.Message;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (ClientBox.SelectedItem is not Client client || ManagerBox.SelectedItem is not AppUser manager)
        {
            ErrorText.Text = "Выберите клиента и менеджера.";
            return;
        }
        if (_items.Count == 0)
        {
            ErrorText.Text = "Добавьте хотя бы один товар.";
            return;
        }
        _sale.ClientId = client.Id;
        _sale.ManagerId = manager.Id;
        _sale.SaleDate = SaleDateBox.SelectedDate ?? DateTime.Today;
        _sale.Discount = DiscountValue;
        _sale.Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Новая";
        _sale.PaymentMethod = (MethodBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Карта";
        try
        {
            Database.SaveSale(_sale, _items.ToList());
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить продажу: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

