using System.Windows;
using System.Windows.Input;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;
using HouseholdGoodsIS.ViewModels;

namespace HouseholdGoodsIS.Views;

public partial class MainWindow : Window
{
    public MainWindow(AppUser account)
    {
        InitializeComponent();
        ViewModel = new MainWindowViewModel(account);
        DataContext = ViewModel;
    }

    public MainWindowViewModel ViewModel { get; }

    private void SearchButton_Click(object sender, RoutedEventArgs e) =>
        ViewModel.NavigateToCatalog(null, ViewModel.GlobalSearch);

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) ViewModel.NavigateToCatalog(null, ViewModel.GlobalSearch);
    }

    private void CartButton_Click(object sender, RoutedEventArgs e) => ViewModel.NavigateToCart();

    private void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        var user = AuthorizationViewModel.CurrentUser;
        if (user == null) return;

        var dialog = new ChangePasswordDialog("Введите текущий пароль и новый пароль.") { Owner = this };
        if (dialog.ShowDialog() != true || string.IsNullOrEmpty(dialog.NewPassword)) return;

        if (!Data.Database.VerifyPassword(user.Id, dialog.CurrentPassword))
        {
            ServiceContainer.Instance.Get<IDialogService>()
                .ShowDialog("Текущий пароль введён неверно.", "Ошибка", DialogType.Error);
            return;
        }

        try
        {
            Data.Database.ChangePassword(user.Id, dialog.NewPassword);
            ServiceContainer.Instance.Get<IDialogService>()
                .ShowDialog("Пароль успешно изменён.", "Готово", DialogType.Info);
        }
        catch (Exception ex)
        {
            ServiceContainer.Instance.Get<IDialogService>()
                .ShowDialog("Не удалось изменить пароль.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        AuthorizationViewModel.LogOut();
        var authorizationWindow = new AuthorizationWindow();
        Application.Current.MainWindow = authorizationWindow;
        authorizationWindow.Show();
        Close();
    }
}
