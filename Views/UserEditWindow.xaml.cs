using System.Windows;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class UserEditWindow : Window
{
    private readonly AppUser _user;
    private readonly bool _isNew;

    public UserEditWindow(AppUser? user)
    {
        InitializeComponent();
        _isNew = user == null;
        _user = user ?? new AppUser();
        TitleText.Text = _isNew ? "Добавление пользователя" : "Редактирование пользователя";
        PasswordLabel.Text = _isNew ? "Пароль *" : "Новый пароль (пусто — без изменений)";
        RoleBox.ItemsSource = Database.GetRoles();
        RoleBox.SelectedIndex = 0;
        if (user == null) return;
        LoginBox.Text = _user.Login;
        LastNameBox.Text = _user.LastName;
        FirstNameBox.Text = _user.FirstName;
        MiddleNameBox.Text = _user.MiddleName ?? string.Empty;
        EmailBox.Text = _user.Email;
        PhoneBox.Text = _user.Phone;
        BlockedCheck.IsChecked = _user.IsBlocked;
        RoleBox.SelectedItem = ((List<Role>)RoleBox.ItemsSource).FirstOrDefault(r => r.Id == _user.RoleId);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(LoginBox.Text) || string.IsNullOrWhiteSpace(LastNameBox.Text)
            || string.IsNullOrWhiteSpace(FirstNameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text)
            || string.IsNullOrWhiteSpace(PhoneBox.Text))
        {
            ErrorText.Text = "Заполните логин, ФИО, почту и телефон.";
            return;
        }
        if (_isNew && PasswordBox.Password.Length < 4)
        {
            ErrorText.Text = "Для нового пользователя задайте пароль (не менее 4 символов).";
            return;
        }
        if (RoleBox.SelectedItem is not Role role)
        {
            ErrorText.Text = "Выберите роль.";
            return;
        }
        _user.Login = LoginBox.Text.Trim();
        _user.LastName = LastNameBox.Text.Trim();
        _user.FirstName = FirstNameBox.Text.Trim();
        _user.MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim();
        _user.Email = EmailBox.Text.Trim();
        _user.Phone = PhoneBox.Text.Trim();
        _user.RoleId = role.Id;
        _user.IsBlocked = BlockedCheck.IsChecked == true;
        try
        {
            Database.SaveUser(_user, PasswordBox.Password.Length > 0 ? PasswordBox.Password : null);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
