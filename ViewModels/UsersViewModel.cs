using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class UsersViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEditorService _editorService;
    private string _searchText = string.Empty;
    private AppUser? _selectedUser;

    public UsersViewModel()
    {
        Title = "Пользователи";
        Glyph = "\uE77B";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();
        AddCommand = new RelayCommand(_ => AddUser());
        EditCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
        DeleteCommand = new RelayCommand(_ => DeleteUser(), _ => SelectedUser != null);
        BlockCommand = new RelayCommand(_ => ToggleBlock(), _ => SelectedUser != null);
        RefreshCommand = new RelayCommand(_ => Load());
        Load();
    }

    public ObservableCollection<AppUser> Users { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) Load(); }
    }

    public AppUser? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public string UsersCountText => $"Найдено пользователей: {Users.Count}";

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand BlockCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public override void Refresh() => Load();

    private void Load()
    {
        try
        {
            var list = Database.GetUsers(SearchText);
            Users.Clear();
            foreach (var user in list) Users.Add(user);
            OnPropertyChanged(nameof(UsersCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить пользователей.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }

    private void AddUser()
    {
        if (_editorService.EditUser(null)) Load();
    }

    private void EditUser()
    {
        if (SelectedUser == null) return;
        if (_editorService.EditUser(SelectedUser)) Load();
    }

    private void DeleteUser()
    {
        if (SelectedUser == null) return;
        if (SelectedUser.Id == AuthorizationViewModel.CurrentUser?.Id)
        {
            _dialogService.ShowDialog("Нельзя удалить текущую учётную запись.", "Действие запрещено", DialogType.Error);
            return;
        }
        var result = _dialogService.ShowDialog($"Удалить пользователя {SelectedUser.FullName}?",
            "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteUser(SelectedUser.Id);
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить пользователя.\n" + ex.Message,
                "Ошибка удаления", DialogType.Error);
        }
    }

    private void ToggleBlock()
    {
        if (SelectedUser == null) return;
        if (SelectedUser.Id == AuthorizationViewModel.CurrentUser?.Id)
        {
            _dialogService.ShowDialog("Нельзя заблокировать текущую учётную запись.", "Действие запрещено", DialogType.Error);
            return;
        }
        var action = SelectedUser.IsBlocked ? "разблокировать" : "заблокировать";
        var result = _dialogService.ShowDialog($"Действительно {action} пользователя {SelectedUser.FullName}?",
            "Подтверждение", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.SetBlocked(SelectedUser.Id, !SelectedUser.IsBlocked);
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось изменить статус блокировки.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }
}
