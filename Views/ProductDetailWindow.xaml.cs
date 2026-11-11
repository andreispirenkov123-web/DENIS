using System.Windows;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.Views;

public partial class ProductDetailWindow : Window
{
    private readonly Product _product;

    public ProductDetailWindow(Product product)
    {
        InitializeComponent();
        _product = product;
        DataContext = product;
        Title = product.Name;
        CategoryText.Text = product.CategoryName;
        NameText.Text = product.Name;
        RatingText.Text = product.RatingText;
        ReviewsText.Text = $"· {product.ReviewsCount} отзывов";
        PriceText.Text = product.PriceText;
        OldPriceText.Text = product.OldPriceText;
        OldPriceText.Visibility = product.HasOldPrice ? Visibility.Visible : Visibility.Collapsed;
        StockText.Text = product.Stock > 0
            ? $"В наличии: {product.Stock} шт. · {product.WarrantyText}"
            : "Нет в наличии";
        StockText.Foreground = (System.Windows.Media.Brush)FindResource(
            product.IsLowStock ? "ErrorBrush" : "SuccessBrush");
        BuyButton.IsEnabled = product.Stock > 0;
        DescriptionText.Text = string.IsNullOrWhiteSpace(product.FullDescription)
            ? product.Description
            : product.FullDescription;

        var attributes = Database.GetProductAttributes(product.Id);
        var groups = attributes
            .GroupBy(a => a.GroupName)
            .Select(g => new AttributeGroup { Name = g.Key, Items = g.ToList() })
            .ToList();
        GroupsList.ItemsSource = groups;
    }

    private void Buy_Click(object sender, RoutedEventArgs e)
    {
        if (CartService.Instance.Add(_product))
            StatusText.Text = "Товар добавлен в корзину.";
        else
            StatusText.Text = $"На складе только {_product.Stock} шт.";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
