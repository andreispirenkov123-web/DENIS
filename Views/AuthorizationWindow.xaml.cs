using System.Windows;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views;

public partial class AuthorizationWindow : Window, IGettingPassword
{
    private readonly AuthorizationViewModel _viewModel;

    public AuthorizationWindow()
    {
        InitializeComponent();
        _viewModel = new AuthorizationViewModel { GettingPassword = this };
        DataContext = _viewModel;
    }

    public string GetPassword() => PasswordBox.Password;

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.LogIn()) return;
        if (AuthorizationViewModel.CurrentUser == null) return;

        var mainWindow = new MainWindow(AuthorizationViewModel.CurrentUser);
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        Close();
    }
}
