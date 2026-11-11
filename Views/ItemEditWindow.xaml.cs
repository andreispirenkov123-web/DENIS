using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class ItemEditWindow : Window
{
    private readonly bool _isSale;
    private readonly List<Product> _products;

    public ItemEditWindow(bool isSale)
    {
        InitializeComponent();
        _isSale = isSale;
        TitleText.Text = isSale ? "Добавление товара в продажу" : "Добавление товара в поставку";
        PriceLabel.Text = isSale ? "Цена продажи, ₽ *" : "Закупочная цена, ₽ *";
        _products = Database.GetProducts();
        ProductBox.ItemsSource = _products;
        ProductBox.SelectedIndex = _products.Count > 0 ? 0 : -1;
    }

    public SaleItem? SaleItem { get; private set; }
    public SupplyItem? SupplyItem { get; private set; }

    private Product? SelectedProduct => ProductBox.SelectedItem as Product;

    private void ProductBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedProduct == null) return;
        PriceBox.Text = (_isSale ? SelectedProduct.RetailPrice : SelectedProduct.PurchasePrice).ToString("0.00");
        StockText.Text = $"Остаток на складе: {SelectedProduct.Stock} шт.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (SelectedProduct == null)
        {
            ErrorText.Text = "Выберите товар.";
            return;
        }
        if (!int.TryParse(QuantityBox.Text, out var quantity) || quantity <= 0)
        {
            ErrorText.Text = "Количество должно быть больше нуля.";
            return;
        }
        if (!decimal.TryParse(PriceBox.Text.Replace(',', '.'), NumberStyles.Any,
                CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            ErrorText.Text = "Введите корректную цену.";
            return;
        }
        if (_isSale && quantity > SelectedProduct.Stock)
        {
            ErrorText.Text = $"Недостаточно товара на складе: остаток {SelectedProduct.Stock} шт.";
            return;
        }
        if (_isSale)
        {
            SaleItem = new SaleItem
            {
                ProductId = SelectedProduct.Id, ProductName = SelectedProduct.Name, Sku = SelectedProduct.Sku,
                Quantity = quantity, UnitPrice = price, TotalPrice = quantity * price
            };
        }
        else
        {
            SupplyItem = new SupplyItem
            {
                ProductId = SelectedProduct.Id, ProductName = SelectedProduct.Name, Sku = SelectedProduct.Sku,
                Quantity = quantity, PurchasePrice = price, TotalPrice = quantity * price
            };
        }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
