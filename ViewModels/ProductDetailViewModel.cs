using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class ProductDetailViewModel : PageViewModelBase
{
    private string _statusMessage = string.Empty;
    private readonly IDialogService _dialogService = ServiceContainer.Instance.Get<IDialogService>();

    public ProductDetailViewModel(Product product)
    {
        Product = product;
        Title = product.Name;
        Glyph = "\uE719";
        Description = string.IsNullOrWhiteSpace(product.FullDescription)
            ? product.Description ?? string.Empty
            : product.FullDescription;

        var attributes = Database.GetProductAttributes(product.Id);
        Groups = attributes
            .GroupBy(a => a.GroupName)
            .Select(g => new AttributeGroup { Name = g.Key, Items = g.ToList() })
            .ToList();

        BackCommand = new RelayCommand(_ => MainVm?.NavigateBack());
        AddToCartCommand = new RelayCommand(_ => AddToCart());
        DeleteCommand = new RelayCommand(_ => DeleteProduct());
    }

    public Product Product { get; }
    public string Description { get; }
    public List<AttributeGroup> Groups { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand AddToCartCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public override void Refresh() { }

    private static MainWindowViewModel? MainVm =>
        System.Windows.Application.Current.MainWindow?.DataContext as MainWindowViewModel;

    private void AddToCart()
    {
        if (CartService.Instance.Add(Product))
            StatusMessage = "Товар добавлен в корзину.";
        else
            StatusMessage = $"На складе только {Product.Stock} шт.";
    }

    private void DeleteProduct()
    {
        var result = _dialogService.ShowDialog($"Удалить товар «{Product.Name}»?",
            "Удаление товара", DialogType.Question);
        if (result != DialogResult.Yes) return;
        try
        {
            Database.DeleteProduct(Product.Id);
            MainVm?.NavigateBack();
            MainVm?.RefreshCurrentPage();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось удалить товар. Возможно, он используется в продажах или поставках.\n"
                                      + ex.Message, "Ошибка удаления", DialogType.Error);
        }
    }
}
