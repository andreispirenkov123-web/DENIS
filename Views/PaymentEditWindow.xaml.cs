using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class PaymentEditWindow : Window
{
    private readonly Payment _payment;

    public PaymentEditWindow(int saleId, Payment? payment)
    {
        InitializeComponent();
        _payment = payment ?? new Payment { SaleId = saleId, PaymentDate = DateTime.Today };
        MethodBox.SelectedIndex = 0;
        DateBox.SelectedDate = _payment.PaymentDate;
        if (payment == null) return;
        AmountBox.Text = _payment.Amount.ToString("0.00");
        PurposeBox.Text = _payment.Purpose ?? string.Empty;
        foreach (var item in MethodBox.Items)
            if (item is ComboBoxItem ci && ci.Content?.ToString() == _payment.PaymentMethod)
            {
                MethodBox.SelectedItem = ci;
                break;
            }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (!decimal.TryParse(AmountBox.Text.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorText.Text = "Введите корректную сумму.";
            return;
        }
        _payment.Amount = amount;
        _payment.PaymentMethod = (MethodBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Карта";
        _payment.PaymentDate = DateBox.SelectedDate ?? DateTime.Today;
        _payment.Purpose = string.IsNullOrWhiteSpace(PurposeBox.Text) ? null : PurposeBox.Text.Trim();
        try
        {
            Database.SavePayment(_payment);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
