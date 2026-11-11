using System.Collections.ObjectModel;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly DashboardViewModel _dashboard;
    private readonly ProductsViewModel _products;
    private readonly CartViewModel _cart;
    private ViewModelBase? _selectedPage;
    private string _globalSearch = string.Empty;

    public MainWindowViewModel(AppUser account)
    {
        Account = account;
        Pages = new ObservableCollection<ViewModelBase>();
        _dashboard = new DashboardViewModel();
        _products = new ProductsViewModel();
        _cart = new CartViewModel();
        BuildPages();
        SelectedPage = Pages.FirstOrDefault();
        CartService.Instance.Changed += (_, _) => OnCartChanged();
    }

    public AppUser Account { get; }
    public ObservableCollection<ViewModelBase> Pages { get; }
    public ViewModelBase? SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (SetProperty(ref _selectedPage, value)) OnPropertyChanged(nameof(PageTitle));
        }
    }

    public string PageTitle => SelectedPage is PageViewModelBase page ? page.Title : "Бытовые товары";
    public string UserFullName => Account.FullName;
    public string RoleName => Account.RoleName;
    public string UserChip => $"{Account.Login} · {Account.RoleName}";
    public string AvatarPath => Account.AvatarPath ?? "images/avatars/default.png";

    public string GlobalSearch
    {
        get => _globalSearch;
        set => SetProperty(ref _globalSearch, value);
    }

    public int CartCount => CartService.Instance.Count;
    public string CartCountText => CartCount.ToString();
    public bool HasCartItems => CartCount > 0;

    private void BuildPages()
    {
        Pages.Add(_dashboard);
        Pages.Add(_products);
        Pages.Add(_cart);
        Pages.Add(new SalesViewModel());
        Pages.Add(new ClientsViewModel());
        Pages.Add(new SuppliesViewModel());
        if (Account.RoleId == 1)
            Pages.Add(new UsersViewModel());
        Pages.Add(new ReportsViewModel());
    }

    public void RefreshCurrentPage()
    {
        if (SelectedPage is PageViewModelBase page) page.Refresh();
    }

    public void NavigateToCatalog(string? categoryName, string? search = null)
    {
        if (search != null) _products.SearchText = search;
        _products.SetCategory(categoryName);
        _products.Refresh();
        SelectedPage = _products;
    }

    public void NavigateToCart()
    {
        _cart.Refresh();
        SelectedPage = _cart;
    }

    public void NavigateToSales()
    {
        var sales = Pages.OfType<SalesViewModel>().FirstOrDefault();
        if (sales == null) return;
        sales.Refresh();
        SelectedPage = sales;
    }

    public void NavigateHome() => SelectedPage = _dashboard;

    private void OnCartChanged()
    {
        OnPropertyChanged(nameof(CartCount));
        OnPropertyChanged(nameof(CartCountText));
        OnPropertyChanged(nameof(HasCartItems));
    }
}
