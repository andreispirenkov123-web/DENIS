using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public interface IGettingPassword
{
    string GetPassword();
}

public class AuthorizationViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private string _login = string.Empty;
    private string _errorMessage = string.Empty;

    public AuthorizationViewModel()
    {
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        LoginCommand = new RelayCommand(_ => LogIn());
        TestConnectionCommand = new RelayCommand(_ => TestConnection());
    }

    public static AppUser? CurrentUser { get; private set; }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password => GettingPassword?.GetPassword() ?? string.Empty;
    public IGettingPassword? GettingPassword { get; set; }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value)) OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public RelayCommand LoginCommand { get; }
    public RelayCommand TestConnectionCommand { get; }

    public bool LogIn()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Поля «Логин» и «Пароль» обязательны для заполнения.";
            return false;
        }
        try
        {
            var user = Database.Authorize(Login.Trim(), Password);
            if (user == null)
            {
                ErrorMessage = "Вы ввели неверный логин или пароль. Пожалуйста, проверьте ещё раз введённые данные.";
                return false;
            }
            if (user.IsBlocked)
            {
                ErrorMessage = "Учётная запись заблокирована. Обратитесь к администратору.";
                return false;
            }
            Database.UpdateLastLogin(user.Id);
            CurrentUser = user;
            return true;
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось подключиться к базе данных.\n" + ex.Message,
                "Ошибка подключения", DialogType.Error);
            return false;
        }
    }

    public static void LogOut() => CurrentUser = null;

    private void TestConnection()
    {
        try
        {
            var version = Database.TestConnection();
            _dialogService.ShowDialog($"Подключение к базе данных установлено успешно.\n{version}",
                "Проверка подключения", DialogType.Info);
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось подключиться к базе данных.\n" + ex.Message,
                "Ошибка подключения", DialogType.Error);
        }
    }
}
