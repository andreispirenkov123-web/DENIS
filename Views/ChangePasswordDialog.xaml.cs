using System.Windows;

namespace HouseholdGoodsIS.Views;

public partial class ChangePasswordDialog : Window
{
    public ChangePasswordDialog(string message)
    {
        InitializeComponent();
        Message = message;
        DataContext = this;
    }

    public string Message { get; }
    public string? NewPassword { get; private set; }
    public string CurrentPassword => CurrentPasswordBox.Password;

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (CurrentPasswordBox.Password.Length == 0 || NewPasswordBox.Password.Length < 4)
        {
            MessageBox.Show("Введите текущий пароль и новый пароль длиной не менее 4 символов.",
                "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        NewPassword = NewPasswordBox.Password;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
