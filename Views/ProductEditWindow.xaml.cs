using System.Globalization;
using System.Windows;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using Microsoft.Win32;

namespace HouseholdGoodsIS.Views;

public partial class ProductEditWindow : Window
{
    private readonly Product _product;

    public ProductEditWindow(Product? product)
    {
        InitializeComponent();
        _product = product ?? new Product();
        TitleText.Text = product == null ? "Добавление товара" : "Редактирование товара";
        CategoryBox.ItemsSource = Database.GetCategories();
        SupplierBox.ItemsSource = Database.GetSuppliers();
        ActiveCheck.IsChecked = true;
        WarrantyBox.Text = "12";
        MinStockBox.Text = "2";
        CategoryBox.SelectedIndex = 0;
        SupplierBox.SelectedIndex = 0;

        if (product == null) return;

        NameBox.Text = _product.Name;
        SkuBox.Text = _product.Sku;
        ManufacturerBox.Text = _product.Manufacturer;
        ModelBox.Text = _product.Model;
        PurchaseBox.Text = _product.PurchasePrice.ToString("0.00");
        RetailBox.Text = _product.RetailPrice.ToString("0.00");
        OldPriceBox.Text = _product.OldPrice?.ToString("0.00") ?? string.Empty;
        WarrantyBox.Text = _product.WarrantyMonths.ToString();
        DescriptionBox.Text = _product.Description ?? string.Empty;
        ImageBox.Text = _product.ImagePath ?? string.Empty;
        ActiveCheck.IsChecked = _product.IsActive;
        MinStockBox.Text = _product.MinStock.ToString();
        CategoryBox.SelectedItem = ((List<Category>)CategoryBox.ItemsSource).FirstOrDefault(c => c.Id == _product.CategoryId);
        SupplierBox.SelectedItem = ((List<Supplier>)SupplierBox.ItemsSource).FirstOrDefault(s => s.Id == _product.SupplierId);
    }

    private static decimal ParseMoney(string text) =>
        decimal.TryParse(text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Изображения (*.png;*.jpg)|*.png;*.jpg" };
        if (dialog.ShowDialog() != true) return;
        var targetDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "products");
        System.IO.Directory.CreateDirectory(targetDir);
        var fileName = System.IO.Path.GetFileName(dialog.FileName);
        try
        {
            System.IO.File.Copy(dialog.FileName, System.IO.Path.Combine(targetDir, fileName), true);
            ImageBox.Text = $"images/products/{fileName}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось скопировать изображение.\n" + ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(SkuBox.Text)
            || string.IsNullOrWhiteSpace(ManufacturerBox.Text) || string.IsNullOrWhiteSpace(ModelBox.Text)
            || CategoryBox.SelectedItem is not Category category || SupplierBox.SelectedItem is not Supplier supplier)
        {
            ErrorText.Text = "Заполните обязательные поля.";
            return;
        }
        var purchase = ParseMoney(PurchaseBox.Text);
        var retail = ParseMoney(RetailBox.Text);
        if (retail <= 0 || purchase < 0)
        {
            ErrorText.Text = "Проверьте закупочную и розничную цену.";
            return;
        }
        if (!int.TryParse(WarrantyBox.Text, out var warranty) || warranty <= 0) warranty = 12;

        _product.Name = NameBox.Text.Trim();
        _product.Sku = SkuBox.Text.Trim();
        _product.CategoryId = category.Id;
        _product.SupplierId = supplier.Id;
        _product.Manufacturer = ManufacturerBox.Text.Trim();
        _product.Model = ModelBox.Text.Trim();
        _product.PurchasePrice = purchase;
        _product.RetailPrice = retail;
        _product.OldPrice = string.IsNullOrWhiteSpace(OldPriceBox.Text) ? null : ParseMoney(OldPriceBox.Text);
        _product.WarrantyMonths = warranty;
        _product.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
        _product.ImagePath = string.IsNullOrWhiteSpace(ImageBox.Text) ? null : ImageBox.Text.Trim();
        if (!int.TryParse(MinStockBox.Text, out var minStock) || minStock < 0) minStock = 2;
        _product.MinStock = minStock;
        _product.IsActive = ActiveCheck.IsChecked == true;

        try
        {
            Database.SaveProduct(_product);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

