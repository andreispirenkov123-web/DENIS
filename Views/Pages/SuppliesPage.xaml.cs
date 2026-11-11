using System.Windows.Controls;
using System.Windows.Input;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views.Pages;

public partial class SuppliesPage : Page
{
    public SuppliesPage() { InitializeComponent(); }

    private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SuppliesViewModel viewModel) viewModel.EditCommand.Execute(null);
    }
}
