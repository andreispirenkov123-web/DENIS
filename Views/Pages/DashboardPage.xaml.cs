using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views.Pages;

public partial class DashboardPage : Page
{
    public DashboardPage() { InitializeComponent(); }

    private void ProductCard_Click_Home(object sender, MouseButtonEventArgs e)
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

    private MainWindowViewModel? MainVm =>
        Application.Current.MainWindow?.DataContext as MainWindowViewModel;

    private void CatalogButton_Click(object sender, RoutedEventArgs e) => MainVm?.NavigateToCatalog(null);

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string category }) MainVm?.NavigateToCatalog(category);
    }
}
