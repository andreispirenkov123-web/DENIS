using System.Windows.Controls;
using System.Windows.Input;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views.Pages;

public partial class SalesPage : Page
{
    public SalesPage() { InitializeComponent(); }

    private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SalesViewModel viewModel) viewModel.EditCommand.Execute(null);
    }
}
