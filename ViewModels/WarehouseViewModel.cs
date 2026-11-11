using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class WarehouseViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private string _searchText = string.Empty;
    private Category? _selectedCategoryItem;
    private bool _lowOnly;
    private List<Product> _rows = new();
    private List<StockMovement> _movements = new();
    private StockSummary _summary = new();
    private Product? _selectedRow;

    public WarehouseViewModel()
    {
        Title = "Склад";
        Glyph = "\uE7B8";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        IncomeCommand = new RelayCommand(_ => Adjust(true), _ => SelectedRow != null);
        WriteOffCommand = new RelayCommand(_ => Adjust(false), _ => SelectedRow != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        LoadCategories();
        Load();
    }

    public ObservableCollection<Category> Categories { get; } = new();
    public List<Product> Rows
    {
        get => _rows;
        private set => SetProperty(ref _rows, value);
    }
    public List<StockMovement> Movements
    {
        get => _movements;
        private set => SetProperty(ref _movements, value);
    }
    public StockSummary Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }
    public Product? SelectedRow
    {
        get => _selectedRow;
        set => SetProperty(ref _selectedRow, value);
    }
    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) Load(); }
    }
    public Category? SelectedCategoryItem
    {
        get => _selectedCategoryItem;
        set { if (SetProperty(ref _selectedCategoryItem, value)) Load(); }
    }
    public bool LowOnly
    {
        get => _lowOnly;
        set { if (SetProperty(ref _lowOnly, value)) Load(); }
    }
    public string RowsCountText => $"Позиций: {Rows.Count}";
    public RelayCommand IncomeCommand { get; }
    public RelayCommand WriteOffCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public override void Refresh()
    {
        LoadCategories();
        Load();
    }

    private void LoadCategories()
    {
        try
        {
            Categories.Clear();
            Categories.Add(new Category { Id = 0, Name = "Все категории" });
            foreach (var category in Database.GetCategoriesWithCounts()) Categories.Add(category);
            SelectedCategoryItem = Categories.FirstOrDefault();
        }
        catch { }
    }

    private void Load()
    {
        try
        {
            var category = SelectedCategoryItem is { Id: > 0 } ? SelectedCategoryItem.Name : string.Empty;
            var list = Database.GetProducts(SearchText, category, 0);
            if (LowOnly) list = list.Where(p => p.IsLowStock).ToList();
            Rows = list;
            Summary = Database.GetStockSummary();
            Movements = Database.GetStockMovements(0, 120);
            OnPropertyChanged(nameof(RowsCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить склад.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }

    private void Adjust(bool income)
    {
        if (SelectedRow == null) return;
        var window = new Views.StockAdjustmentWindow(SelectedRow, income)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (window.ShowDialog() == true) Load();
    }
}
