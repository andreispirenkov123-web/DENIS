using System.Collections.ObjectModel;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class ProductsViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IEditorService _editorService;
    private string _searchText = string.Empty;
    private string _selectedCategory = "Все категории";
    private int _selectedSortIndex;
    private Product? _selectedProduct;

    public ProductsViewModel()
    {
        Title = "Товары";
        Glyph = "\uE719";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();
        AddCommand = new RelayCommand(_ => AddProduct());
        EditCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
        DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
        RefreshCommand = new RelayCommand(_ => Load());
        LoadCategories();
        Load();
    }

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<string> Categories { get; } = new() { "Все категории" };
    public ObservableCollection<string> Sorts { get; } = new()
    {
        "По названию (А–Я)", "По цене (возрастание)", "По цене (убывание)",
        "По остатку (возрастание)", "По названию (Я–А)"
    };

    public string SearchText
    {
        get => _searchText;
        set { if (SetProperty(ref _searchText, value)) Load(); }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { if (SetProperty(ref _selectedCategory, value)) Load(); }
    }

    public int SelectedSortIndex
    {
        get => _selectedSortIndex;
        set { if (SetProperty(ref _selectedSortIndex, value)) Load(); }
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public string ProductsCountText => $"Найдено товаров: {Products.Count}";

    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
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
            var saved = SelectedCategory;
            Categories.Clear();
            Categories.Add("Все категории");
            foreach (var category in Database.GetCategories()) Categories.Add(category.Name);
            SelectedCategory = Categories.Contains(saved) ? saved : "Все категории";
        }
        catch
        {
            // категории загрузятся при следующем обновлении
        }
    }

    private void Load()
    {
        try
        {
            var category = SelectedCategory == "Все категории" ? string.Empty : SelectedCategory;
            var list = Database.GetProducts(SearchText, category, SelectedSortIndex);
            Products.Clear();
            foreach (var product in list) Products.Add(product);
            OnPropertyChanged(nameof(ProductsCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить список товаров.\n" + ex.Message,
                "Ошибка чтения данных", DialogType.Error);
        }
    }

    private void AddProduct()
    {
        if (_editorService.EditProduct(null))
        {
            LoadCategories();
            Load();
        }
    }

    private void EditProduct()
    {
        if (SelectedProduct == null) return;
        if (_editorService.EditProduct(SelectedProduct)) Load();
    }

    private void DeleteProduct()
    {
        if (SelectedProduct == null) return;
        var result = _dialogService.ShowDialog(
            $"Удалить товар «{SelectedProduct.Name}»?", "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteProduct(SelectedProduct.Id);
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить товар. Возможно, он используется в продажах или поставках.\n"
                                      + ex.Message, "Ошибка удаления", DialogType.Error);
        }
    }
}
