using System.Collections.ObjectModel;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase? _selectedPage;

    public MainWindowViewModel(AppUser account)
    {
        Account = account;
        Pages = new ObservableCollection<ViewModelBase>();
        BuildPages();
        SelectedPage = Pages.FirstOrDefault();
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

    private void BuildPages()
    {
        Pages.Add(new DashboardViewModel());
        Pages.Add(new ProductsViewModel());
        Pages.Add(new SalesViewModel());
        Pages.Add(new ClientsViewModel());
        Pages.Add(new SuppliesViewModel());
        Pages.Add(new ReferencesViewModel());
        if (Account.RoleId == 1)
            Pages.Add(new UsersViewModel());
        Pages.Add(new ReportsViewModel());
    }

    public void RefreshCurrentPage()
    {
        if (SelectedPage is PageViewModelBase page) page.Refresh();
    }
}
