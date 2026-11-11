using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class SalesViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEditorService _editorService;
    private string _searchText = string.Empty;
    private string _selectedStatus = "Все статусы";
    private Sale? _selectedSale;

    public SalesViewModel()
    {
        Title = "Заказы";
        Glyph = "\uE7BF";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();
        AddCommand = new RelayCommand(_ => AddSale());
        EditCommand = new RelayCommand(_ => EditSale(), _ => SelectedSale != null);
        DeleteCommand = new RelayCommand(_ => DeleteSale(), _ => SelectedSale != null);
        RefreshCommand = new RelayCommand(_ => Load());
        Load();
    }

    public ObservableCollection<Sale> Sales { get; } = new();
    public ObservableCollection<string> Statuses { get; } = new()
    {
        "Все статусы", "Новая", "Оплачена", "Собрана", "Выдана", "Отменена"
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

    public Sale? SelectedSale
    {
        get => _selectedSale;
        set => SetProperty(ref _selectedSale, value);
    }

    public string SalesCountText => $"Найдено продаж: {Sales.Count}";

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
            var list = Database.GetSales(SearchText, status);
            Sales.Clear();
            foreach (var sale in list) Sales.Add(sale);
            OnPropertyChanged(nameof(SalesCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить список продаж.\n" + ex.Message,
                "Ошибка чтения данных", DialogType.Error);
        }
    }

    private void AddSale()
    {
        if (_editorService.EditSale(null)) Load();
    }

    private void EditSale()
    {
        if (SelectedSale == null) return;
        if (_editorService.EditSale(SelectedSale)) Load();
    }

    private void DeleteSale()
    {
        if (SelectedSale == null) return;
        var result = _dialogService.ShowDialog(
            $"Удалить продажу №{SelectedSale.Id}?", "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteSale(SelectedSale.Id);
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить продажу.\n" + ex.Message,
                "Ошибка удаления", DialogType.Error);
        }
    }
}
