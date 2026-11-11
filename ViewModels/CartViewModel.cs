using System.Collections.ObjectModel;
using System.Globalization;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class CartViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private Store? _selectedStore;
    private int _selectedPaymentIndex;

    public CartViewModel()
    {
        Title = "Корзина";
        Glyph = "\uE7BF";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        IncreaseCommand = new RelayCommand(p => { if (p is CartItem item) CartService.Instance.Increase(item); });
        DecreaseCommand = new RelayCommand(p => { if (p is CartItem item) CartService.Instance.Decrease(item); });
        RemoveCommand = new RelayCommand(p => { if (p is CartItem item) CartService.Instance.Remove(item); });
        ClearCommand = new RelayCommand(_ => CartService.Instance.Clear());
        CheckoutCommand = new RelayCommand(_ => Checkout());
        ContinueCommand = new RelayCommand(_ => NavigateCatalog());
        Stores = new ObservableCollection<Store>();
        PaymentMethods = new ObservableCollection<string> { "Карта", "СБП", "Наличные" };
        SelectedPaymentIndex = 0;
        CartService.Instance.Changed += (_, _) => UpdateSummary();
        UpdateSummary();
    }

    public ObservableCollection<CartItem> Items => CartService.Instance.Items;
    public ObservableCollection<Store> Stores { get; }
    public ObservableCollection<string> PaymentMethods { get; }

    public Store? SelectedStore
    {
        get => _selectedStore;
        set => SetProperty(ref _selectedStore, value);
    }

    public string? SelectedStoreInfo => SelectedStore == null
        ? null
        : $"{SelectedStore.Address} · {SelectedStore.WorkingHours}";

    public int SelectedPaymentIndex
    {
        get => _selectedPaymentIndex;
        set => SetProperty(ref _selectedPaymentIndex, value);
    }

    public string PaymentMethod => SelectedPaymentIndex >= 0 && SelectedPaymentIndex < PaymentMethods.Count
        ? PaymentMethods[SelectedPaymentIndex] : "Карта";

    public bool HasItems => CartService.Instance.Items.Count > 0;
    public bool IsEmpty => !HasItems;
    public string TotalText
    {
        get
        {
            var total = CartService.Instance.Total;
            return total.ToString("N0", CultureInfo.GetCultureInfo("ru-RU")) + " ₽";
        }
    }
    public string CountText => $"Товаров в корзине: {CartService.Instance.Count}";

    public RelayCommand IncreaseCommand { get; }
    public RelayCommand DecreaseCommand { get; }
    public RelayCommand RemoveCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand CheckoutCommand { get; }
    public RelayCommand ContinueCommand { get; }

    public override void Refresh()
    {
        LoadStores();
        UpdateSummary();
    }

    private void LoadStores()
    {
        try
        {
            var list = Database.GetStores();
            Stores.Clear();
            foreach (var store in list) Stores.Add(store);
            if (SelectedStore == null && Stores.Count > 0) SelectedStore = Stores[0];
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить магазины.\n" + ex.Message,
                "Ошибка", DialogType.Error);
        }
    }

    private void UpdateSummary()
    {
        OnPropertyChanged(nameof(HasItems));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(TotalText));
        OnPropertyChanged(nameof(CountText));
        if (Stores.Count == 0) LoadStores();
    }

    private void Checkout()
    {
        if (CartService.Instance.Items.Count == 0)
        {
            _dialogService.ShowDialog("Корзина пуста. Добавьте товары из каталога.",
                "Корзина", DialogType.Error);
            return;
        }
        if (SelectedStore == null)
        {
            _dialogService.ShowDialog("Выберите магазин самовывоза.", "Оформление заказа", DialogType.Error);
            return;
        }

        var sale = new Sale
        {
            PickupStoreId = SelectedStore.Id,
            ManagerId = AuthorizationViewModel.CurrentUser?.Id ?? 1,
            SaleDate = DateTime.Today,
            Status = "Оплачена",
            PaymentMethod = PaymentMethod,
            Discount = 0
        };
        var items = CartService.Instance.Items.Select(item => new SaleItem
        {
            ProductId = item.ProductId,
            ProductName = item.Name,
            Sku = item.Sku,
            Quantity = item.Quantity,
            UnitPrice = item.Product.RetailPrice,
            TotalPrice = item.Sum
        }).ToList();

        try
        {
            Database.SaveSale(sale, items);
            Database.SavePayment(new Payment
            {
                SaleId = sale.Id,
                Amount = sale.TotalAmount,
                PaymentMethod = PaymentMethod,
                PaymentDate = DateTime.Today,
                Purpose = "Оплата заказа"
            });
            CartService.Instance.Clear();
            _dialogService.ShowDialog($"Заказ №{sale.Id} успешно оформлен.\nСумма: {sale.TotalAmount:N0} ₽",
                "Заказ оформлен", DialogType.Info);
            NavigateSales();
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось оформить заказ.\n" + ex.Message,
                "Ошибка", DialogType.Error);
        }
    }

    private static MainWindowViewModel? MainVm =>
        System.Windows.Application.Current.MainWindow?.DataContext as MainWindowViewModel;

    private static void NavigateCatalog() => MainVm?.NavigateToCatalog(null);
    private static void NavigateSales() => MainVm?.NavigateToSales();
}
