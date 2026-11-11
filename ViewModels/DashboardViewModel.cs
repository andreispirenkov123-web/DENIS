using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.ViewModels;

public class DashboardViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private DashboardStats _stats = new();
    private List<StatusCount> _statusCounts = new();
    private List<Product> _lowStock = new();
    private List<Sale> _recentSales = new();

    public DashboardViewModel()
    {
        Title = "Главная";
        Glyph = "\uE80F";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        Load();
    }

    public DashboardStats Stats
    {
        get => _stats;
        private set => SetProperty(ref _stats, value);
    }

    public List<StatusCount> StatusCounts
    {
        get => _statusCounts;
        private set => SetProperty(ref _statusCounts, value);
    }

    public List<Product> LowStock
    {
        get => _lowStock;
        private set => SetProperty(ref _lowStock, value);
    }

    public List<Sale> RecentSales
    {
        get => _recentSales;
        private set => SetProperty(ref _recentSales, value);
    }

    public override void Refresh() => Load();

    private void Load()
    {
        try
        {
            Stats = Database.GetDashboardStats();
            StatusCounts = Database.GetSalesByStatus();
            LowStock = Database.GetLowStock(5);
            RecentSales = Database.GetRecentSales(5);
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось загрузить показатели.\n" + ex.Message,
                "Ошибка чтения данных", DialogType.Error);
        }
    }
}
