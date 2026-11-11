using System.Windows;
using System.Windows.Controls;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views.Pages;

public partial class DashboardPage : Page
{
    public DashboardPage() { InitializeComponent(); }

    private MainWindowViewModel? MainVm =>
        Application.Current.MainWindow?.DataContext as MainWindowViewModel;

    private void CatalogButton_Click(object sender, RoutedEventArgs e) => MainVm?.NavigateToCatalog(null);

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string category }) MainVm?.NavigateToCatalog(category);
    }
}
