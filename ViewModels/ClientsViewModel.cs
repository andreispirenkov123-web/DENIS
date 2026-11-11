using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class ClientsViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEditorService _editorService;
    private string _searchText = string.Empty;
    private Client? _selectedClient;

    public ClientsViewModel()
    {
        Title = "Клиенты";
        Glyph = "\uE716";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();
        AddCommand = new RelayCommand(_ => AddClient());
        EditCommand = new RelayCommand(_ => EditClient(), _ => SelectedClient != null);
        DeleteCommand = new RelayCommand(_ => DeleteClient(), _ => SelectedClient != null);
        RefreshCommand = new RelayCommand(_ => Load());
        Load();
    }

    public ObservableCollection<Client> Clients { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) Load(); }
    }

    public Client? SelectedClient
    {
        get => _selectedClient;
        set => SetProperty(ref _selectedClient, value);
    }

    public string ClientsCountText => $"Найдено клиентов: {Clients.Count}";

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public override void Refresh() => Load();

    private void Load()
    {
        try
        {
            var list = Database.GetClients(SearchText);
            Clients.Clear();
            foreach (var client in list) Clients.Add(client);
            OnPropertyChanged(nameof(ClientsCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить список клиентов.\n" + ex.Message,
                "Ошибка чтения данных", DialogType.Error);
        }
    }

    private void AddClient()
    {
        if (_editorService.EditClient(null)) Load();
    }

    private void EditClient()
    {
        if (SelectedClient == null) return;
        if (_editorService.EditClient(SelectedClient)) Load();
    }

    private void DeleteClient()
    {
        if (SelectedClient == null) return;
        var result = _dialogService.ShowDialog(
            $"Удалить клиента {SelectedClient.FullName}?", "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteClient(SelectedClient.Id);
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить клиента. Возможно, у него есть продажи.\n" + ex.Message,
                "Ошибка удаления", DialogType.Error);
        }
    }
}
