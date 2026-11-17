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
    private Category? _selectedCategoryItem;
    private int _selectedSortIndex;
    private Product? _selectedProduct;

    public ProductsViewModel()
    {
        Title = "Каталог";
        Glyph = "\uE719";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        _editorService = ServiceContainer.Instance.Get<IEditorService>();
        AddCommand = new RelayCommand(_ => AddProduct());
        EditCommand = new RelayCommand(_ => EditSelected(), _ => SelectedProduct != null);
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedProduct != null);
        RefreshCommand = new RelayCommand(_ => Refresh());
        AddToCartCommand = new RelayCommand(parameter => AddToCart(parameter as Product));
        EditProductCommand = new RelayCommand(parameter => EditProduct(parameter as Product));
        DeleteProductCommand = new RelayCommand(parameter => DeleteProduct(parameter as Product));
        LoadCategories();
        Load();
    }

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<string> Sorts { get; } = new()
    {
        "По популярности", "По цене (возрастание)", "По цене (убывание)",
        "По остатку (возрастание)", "По названию (А–Я)"
    };

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
    public RelayCommand AddToCartCommand { get; }
    public RelayCommand EditProductCommand { get; }
    public RelayCommand DeleteProductCommand { get; }

    public override void Refresh()
    {
        LoadCategories();
        Load();
    }

    public void SetCategory(string? categoryName)
    {
        var target = string.IsNullOrWhiteSpace(categoryName)
            ? null
            : Categories.FirstOrDefault(c => c.Name == categoryName);
        SetProperty(ref _selectedCategoryItem, target, nameof(SelectedCategoryItem));
    }

    private void LoadCategories()
    {
        try
        {
            var selected = SelectedCategoryItem?.Name;
            Categories.Clear();
            var categories = Database.GetCategoriesWithCounts();
            Categories.Add(new Category { Id = 0, Name = "Все категории",
                ProductCount = categories.Sum(c => c.ProductCount) });
            foreach (var category in categories) Categories.Add(category);
            SelectedCategoryItem = Categories.FirstOrDefault(c => c.Name == selected)
                                   ?? Categories.FirstOrDefault();
        }
        catch
        {
        }
    }

    private void Load()
    {
        try
        {
            var category = SelectedCategoryItem is { Id: > 0 } ? SelectedCategoryItem.Name : string.Empty;
            var list = Database.GetProducts(SearchText, category, SelectedSortIndex);
            Products.Clear();
            foreach (var product in list) Products.Add(product);
            OnPropertyChanged(nameof(ProductsCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить каталог товаров.\n" + ex.Message,
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

    private void EditSelected()
    {
        if (SelectedProduct != null) EditProduct(SelectedProduct);
    }

    private void DeleteSelected()
    {
        if (SelectedProduct != null) DeleteProduct(SelectedProduct);
    }

    private void EditProduct(Product? product)
    {
        if (product == null) return;
        if (_editorService.EditProduct(product))
        {
            LoadCategories();
            Load();
        }
    }

    private void DeleteProduct(Product? product)
    {
        if (product == null) return;
        var result = _dialogService.ShowDialog($"Удалить товар «{product.Name}»?",
            "Подтверждение удаления", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteProduct(product.Id);
            LoadCategories();
            Load();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить товар. Возможно, он используется в продажах или поставках.\n"
                                      + ex.Message, "Ошибка удаления", DialogType.Error);
        }
    }

    private void AddToCart(Product? product)
    {
        if (product == null) return;
        if (!CartService.Instance.Add(product))
        {
            _dialogService.ShowDialog(product.Stock <= 0
                    ? "Товара нет в наличии."
                    : $"На складе только {product.Stock} шт. Больше добавить нельзя.",
                "Корзина", DialogType.Error);
        }
    }
}
