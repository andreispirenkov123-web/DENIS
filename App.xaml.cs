using System.IO;
using System.Windows;
using HouseholdGoodsIS.Services;
using HouseholdGoodsIS.ViewModels;
using HouseholdGoodsIS.Views;

namespace HouseholdGoodsIS;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            LogException("AppDomain", args.ExceptionObject as Exception);
        DispatcherUnhandledException += (_, args) =>
        {
            LogException("Dispatcher", args.Exception);
            args.Handled = true;
        };

        ServiceContainer.Instance.Add<IDialogService>(new DialogService());
        ServiceContainer.Instance.Add<IEditorService>(new EditorService());

        if (e.Args.Length >= 2 && e.Args[0] == "--screenshots")
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            ScreenshotService.CaptureAll(e.Args[1]);
            Shutdown();
            return;
        }

        if (e.Args.Length >= 1 && e.Args[0] == "--selftest")
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            RunSelfTest();
            Shutdown();
            return;
        }

        var authorizationWindow = new AuthorizationWindow();
        MainWindow = authorizationWindow;
        authorizationWindow.Show();
    }

    private static void RunSelfTest()
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "selftest.log");
        try
        {
            var admin = Data.Database.Authorize("admin", "admin123");
            var products = Data.Database.GetProducts();
            if (admin == null || products.Count == 0)
                throw new InvalidOperationException("Нет пользователя или товаров");

            var product = products[0];
            Services.CartService.Instance.Add(product, 1);
            var items = Services.CartService.Instance.Items.Select(i => new Models.SaleItem
            {
                ProductId = i.ProductId,
                ProductName = i.Name,
                Sku = i.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.Product.RetailPrice,
                TotalPrice = i.Sum
            }).ToList();

            var sale = new Models.Sale
            {
                ClientId = Data.Database.GetClients().First().Id,
                ManagerId = admin.Id,
                SaleDate = DateTime.Today,
                Status = "Оплачена",
                PaymentMethod = "Карта",
                Discount = 0
            };
            Data.Database.SaveSale(sale, items);
            Data.Database.SavePayment(new Models.Payment
            {
                SaleId = sale.Id,
                Amount = sale.TotalAmount,
                PaymentMethod = "Карта",
                PaymentDate = DateTime.Today,
                Purpose = "Оплата заказа (автотест)"
            });
            Services.CartService.Instance.Clear();
            File.WriteAllText(logPath,
                $"OK sale_id={sale.Id} total={sale.TotalAmount} items={items.Count}");
        }
        catch (Exception ex)
        {
            File.WriteAllText(logPath, "FAIL " + ex);
        }
    }

    private static void LogException(string source, Exception? exception)
    {
        try
        {
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crash.log"),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch { }
    }
}

