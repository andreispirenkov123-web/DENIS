using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class SuppliesViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEditorService _editorService;
    private string _searchText = string.Empty;
    private string _selectedStatus = "Все статусы";
    private Supply? _selectedSupply;

    public SuppliesViewModel()
    {
        Title = "Поставки";
        Glyph = "\uE7B8";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();
        AddCommand = new RelayCommand(_ => AddSupply());
        EditCommand = new RelayCommand(_ => EditSupply(), _ => SelectedSupply != null);
        DeleteCommand = new RelayCommand(_ => DeleteSupply(), _ => SelectedSupply != null);
        RefreshCommand = new RelayCommand(_ => Load());
        Load();
    }

    public ObservableCollection<Supply> Supplies { get; } = new();
    public ObservableCollection<string> Statuses { get; } = new()
    {
        "Все статусы", "Оформлена", "В пути", "Получена", "Отменена"
    };

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) Load(); }
    }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set { if (SetProperty(ref _selectedStatus, value)) Load(); }
    }

    public Supply? SelectedSupply
    {
        get => _selectedSupply;
        set => SetProperty(ref _selectedSupply, value);
    }

    public string SuppliesCountText => $"Найдено поставок: {Supplies.Count}";

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public override void Refresh() => Load();

    private void Load()
    {
        try
        {
            var status = SelectedStatus == "Все статусы" ? string.Empty : SelectedStatus;
            var list = Database.GetSupplies(SearchText, status);
            Supplies.Clear();
            foreach (var supply in list) Supplies.Add(supply);
            OnPropertyChanged(nameof(SuppliesCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить список поставок.\n" + ex.Message,
                "Ошибка чтения данных", DialogType.Error);
        }
    }

    private void AddSupply()
    {
        if (_editorService.EditSupply(null)) Load();
    }

    private void EditSupply()
    {
        if (SelectedSupply == null) return;
        if (_editorService.EditSupply(SelectedSupply)) Load();
    }

    private void DeleteSupply()
    {
        if (SelectedSupply == null) return;
        var result = _dialogService.ShowDialog(
            $"Удалить поставку {SelectedSupply.InvoiceNumber}?", "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteSupply(SelectedSupply.Id);
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить поставку.\n" + ex.Message,
                "Ошибка удаления", DialogType.Error);
        }
    }
}
