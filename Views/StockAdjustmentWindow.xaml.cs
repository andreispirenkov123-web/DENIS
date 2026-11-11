using System.Windows;
using System.Windows.Controls;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class StockAdjustmentWindow : Window
{
    private readonly Product _product;
    private readonly bool _income;

    public StockAdjustmentWindow(Product product, bool income)
    {
        InitializeComponent();
        _product = product;
        _income = income;
        Title = income ? "Оприходование товара" : "Списание товара";
        TitleText.Text = income ? "Оприходование товара" : "Списание товара";
        SaveButton.Content = income ? "Оприходовать" : "Списать";
        ProductText.Text = $"{product.Name} ({product.Sku})";
        StockText.Text = $"Текущий остаток: {product.Stock} шт. · минимальный: {product.MinStock} шт.";
        ReasonBox.SelectedIndex = income ? 0 : 1;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (!int.TryParse(QuantityBox.Text, out var quantity) || quantity <= 0)
        {
            ErrorText.Text = "Введите количество больше нуля.";
            return;
        }
        if (!_income && quantity > _product.Stock)
        {
            ErrorText.Text = $"Нельзя списать больше остатка ({_product.Stock} шт.).";
            return;
        }
        var reason = (ReasonBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Прочее";
        var comment = string.IsNullOrWhiteSpace(CommentBox.Text) ? null : CommentBox.Text.Trim();
        try
        {
            Database.SaveStockAdjustment(_product.Id, _income ? quantity : -quantity, reason, comment,
                ViewModels.AuthorizationViewModel.CurrentUser?.Id ?? 1);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
