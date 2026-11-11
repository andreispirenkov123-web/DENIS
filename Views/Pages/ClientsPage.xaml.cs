using System.Windows.Controls;
using System.Windows.Input;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views.Pages;

public partial class ClientsPage : Page
{
    public ClientsPage() { InitializeComponent(); }

    private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ClientsViewModel viewModel) viewModel.EditCommand.Execute(null);
    }
}
