using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class DashboardViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private List<Category> _categories = new();
    private List<Product> _popularProducts = new();
    private DashboardStats _stats = new();

    public DashboardViewModel()
    {
        Title = "Главная";
        Glyph = "\uE80F";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        AddToCartCommand = new RelayCommand(parameter => AddToCart(parameter as Product));
        Load();
    }

    public List<Category> Categories
    {
        get => _categories;
        private set => SetProperty(ref _categories, value);
    }

    public List<Product> PopularProducts
    {
        get => _popularProducts;
        private set => SetProperty(ref _popularProducts, value);
    }

    public DashboardStats Stats
    {
        get => _stats;
        private set => SetProperty(ref _stats, value);
    }

    public RelayCommand AddToCartCommand { get; }

    public override void Refresh() => Load();

    private void Load()
    {
        try
        {
            Categories = Database.GetCategoriesWithCounts();
            PopularProducts = Database.GetPopularProducts(8);
            Stats = Database.GetDashboardStats();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить главную страницу.\n" + ex.Message,
                "Ошибка чтения данных", DialogType.Error);
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
