using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views.Pages;

public partial class ProductsPage : Page
{
    public ProductsPage() { InitializeComponent(); }

    private void ProductCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (FindParent<Button>(e.OriginalSource as DependencyObject) != null) return;
        if ((sender as FrameworkElement)?.DataContext is not Product product) return;
        var window = new ProductDetailWindow(product) { Owner = Window.GetWindow(this) };
        window.ShowDialog();
    }

    private static T? FindParent<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source != null)
        {
            if (source is T result) return result;
            source = VisualTreeHelper.GetParent(source);
        }
        return null;
    }
}
