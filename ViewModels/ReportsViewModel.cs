using System.IO;
using System.Text;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;
using Microsoft.Win32;

namespace HouseholdGoodsIS.ViewModels;

public class ReportsViewModel : PageViewModelBase
{
    private readonly IDialogService _dialogService;
    private DateTime _fromDate = new(DateTime.Today.Year, 1, 1);
    private DateTime _toDate = DateTime.Today;
    private List<CategoryTotal> _categoryRows = new();
    private List<ProductTotal> _topProducts = new();
    private List<StoreTotal> _topStores = new();
    private string _summary = string.Empty;

    public ReportsViewModel()
    {
        Title = "Отчёты";
        Glyph = "\uE9D2";
        _dialogService = ServiceContainer.Instance.Get<IDialogService>();
        BuildCommand = new RelayCommand(_ => Load());
        ExportCommand = new RelayCommand(_ => Export());
        Load();
    }

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public List<CategoryTotal> CategoryRows
    {
        get => _categoryRows;
        private set => SetProperty(ref _categoryRows, value);
    }

    public List<ProductTotal> TopProducts
    {
        get => _topProducts;
        private set => SetProperty(ref _topProducts, value);
    }

    public List<StoreTotal> TopStores
    {
        get => _topStores;
        private set => SetProperty(ref _topStores, value);
    }

    public string Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public RelayCommand BuildCommand { get; }
    public RelayCommand ExportCommand { get; }

    public override void Refresh() => Load();

    private void Load()
    {
        if (FromDate > ToDate)
        {
            _dialogService.ShowDialog("Дата начала периода не может быть больше даты окончания.",
                "Проверка периода", DialogType.Error);
            return;
        }
        try
        {
            CategoryRows = Database.GetSalesByCategory(FromDate, ToDate);
            TopProducts = Database.GetTopProducts(FromDate, ToDate);
            TopStores = Database.GetTopStores(FromDate, ToDate);
            var sales = CategoryRows.Sum(r => r.SalesCount);
            var sum = CategoryRows.Sum(r => r.TotalSum);
            Summary = $"Период: {FromDate:dd.MM.yyyy} – {ToDate:dd.MM.yyyy}   |   Продаж: {sales}   |   Сумма: {sum:N0} ₽";
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось сформировать отчёт.\n" + ex.Message, "Ошибка", DialogType.Error);
        }
    }

    private void Export()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"Отчёт_бытовые_товары_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}.csv"
        };
        if (dialog.ShowDialog() != true) return;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Категория;Продаж;Товаров;Сумма, руб.");
            foreach (var row in CategoryRows)
                sb.AppendLine($"{row.CategoryName};{row.SalesCount};{row.ItemsCount};{row.TotalSum:F2}");
            sb.AppendLine();
            sb.AppendLine("Товар;Количество;Сумма, руб.");
            foreach (var item in TopProducts)
                sb.AppendLine($"{item.ProductName};{item.Quantity};{item.TotalSum:F2}");
            sb.AppendLine();
            sb.AppendLine("Магазин;Продаж;Сумма, руб.");
            foreach (var store in TopStores)
                sb.AppendLine($"{store.StoreName};{store.SalesCount};{store.TotalSum:F2}");
            File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
            _dialogService.ShowDialog("Отчёт сохранён в файл:\n" + dialog.FileName, "Экспорт завершён", DialogType.Info);
        }
        catch (Exception ex)
        {
            _dialogService.ShowDialog("Не удалось сохранить отчёт.\n" + ex.Message, "Ошибка экспорта", DialogType.Error);
        }
    }
}
