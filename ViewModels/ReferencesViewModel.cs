using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class ReferencesViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEditorService _editorService;
    private string _categorySearch = string.Empty;
    private string _supplierSearch = string.Empty;
    private Category? _selectedCategory;
    private Supplier? _selectedSupplier;

    public ReferencesViewModel()
    {
        Title = "Справочники";
        Glyph = "\uE8F1";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();

        AddCategoryCommand = new RelayCommand(_ => AddCategory());
        EditCategoryCommand = new RelayCommand(_ => EditCategory(), _ => SelectedCategory != null);
        DeleteCategoryCommand = new RelayCommand(_ => DeleteCategory(), _ => SelectedCategory != null);
        AddSupplierCommand = new RelayCommand(_ => AddSupplier());
        EditSupplierCommand = new RelayCommand(_ => EditSupplier(), _ => SelectedSupplier != null);
        DeleteSupplierCommand = new RelayCommand(_ => DeleteSupplier(), _ => SelectedSupplier != null);
        RefreshCommand = new RelayCommand(_ => Load());
        Load();
    }

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Supplier> Suppliers { get; } = new();

    public string CategorySearch
    {
        get => _categorySearch;
        set { if (SetProperty(ref _categorySearch, value)) LoadCategories(); }
    }

    public string SupplierSearch
    {
        get => _supplierSearch;
        set { if (SetProperty(ref _supplierSearch, value)) LoadSuppliers(); }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public Supplier? SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetProperty(ref _selectedSupplier, value);
    }

    public RelayCommand AddCategoryCommand { get; }
    public RelayCommand EditCategoryCommand { get; }
    public RelayCommand DeleteCategoryCommand { get; }
    public RelayCommand AddSupplierCommand { get; }
    public RelayCommand EditSupplierCommand { get; }
    public RelayCommand DeleteSupplierCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public override void Refresh() => Load();

    private void Load()
    {
        LoadCategories();
        LoadSuppliers();
    }

    private void LoadCategories()
    {
        try
        {
            var list = Database.GetCategories()
                .Where(c => string.IsNullOrWhiteSpace(CategorySearch)
                            || c.Name.Contains(CategorySearch, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Categories.Clear();
            foreach (var category in list) Categories.Add(category);
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить категории.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }

    private void LoadSuppliers()
    {
        try
        {
            var list = Database.GetSuppliers()
                .Where(s => string.IsNullOrWhiteSpace(SupplierSearch)
                            || s.Name.Contains(SupplierSearch, StringComparison.OrdinalIgnoreCase)
                            || (s.City ?? string.Empty).Contains(SupplierSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Suppliers.Clear();
            foreach (var supplier in list) Suppliers.Add(supplier);
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить поставщиков.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }

    private void AddCategory()
    {
        if (_editorService.EditCategory(null)) LoadCategories();
    }

    private void EditCategory()
    {
        if (SelectedCategory == null) return;
        if (_editorService.EditCategory(SelectedCategory)) LoadCategories();
    }

    private void DeleteCategory()
    {
        if (SelectedCategory == null) return;
        var result = _dialogService.ShowDialog($"Удалить категорию «{SelectedCategory.Name}»?",
            "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteCategory(SelectedCategory.Id);
            LoadCategories();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить категорию. Возможно, в ней есть товары.\n" + ex.Message,
                "Ошибка удаления", DialogType.Error);
        }
    }

    private void AddSupplier()
    {
        if (_editorService.EditSupplier(null)) LoadSuppliers();
    }

    private void EditSupplier()
    {
        if (SelectedSupplier == null) return;
        if (_editorService.EditSupplier(SelectedSupplier)) LoadSuppliers();
    }

    private void DeleteSupplier()
    {
        if (SelectedSupplier == null) return;
        var result = _dialogService.ShowDialog($"Удалить поставщика «{SelectedSupplier.Name}»?",
            "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteSupplier(SelectedSupplier.Id);
            LoadSuppliers();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить поставщика. Возможно, у него есть товары.\n" + ex.Message,
                "Ошибка удаления", DialogType.Error);
        }
    }
}
