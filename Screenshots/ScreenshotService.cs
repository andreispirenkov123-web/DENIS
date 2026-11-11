using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Services;
using HouseholdGoodsIS.ViewModels;
using HouseholdGoodsIS.Views;

namespace HouseholdGoodsIS;

/// <summary>Служебный режим формирования скриншотов для документации.</summary>
public static class ScreenshotService
{
    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }

    public static void CaptureAll(string directory)
    {
        Directory.CreateDirectory(directory);
        var admin = Database.Authorize("admin", "admin123");
        var manager = Database.Authorize("manager", "manager123");
        if (admin == null || manager == null) return;

        var auth = new AuthorizationWindow { Width = 480, Height = 680 };
        var authVm = (AuthorizationViewModel)auth.DataContext;
        authVm.Login = "admin";
        auth.PasswordBox.Password = "admin123";
        CaptureWindow(auth, Path.Combine(directory, "01_authorization.png"));

        var authError = new AuthorizationWindow { Width = 480, Height = 680 };
        var authErrorVm = (AuthorizationViewModel)authError.DataContext;
        authErrorVm.Login = "admin";
        authError.PasswordBox.Password = "wrong-password";
        authErrorVm.LogIn();
        CaptureWindow(authError, Path.Combine(directory, "02_authorization_error.png"));

        CaptureWindow(new Dialog("Подключение к базе данных установлено успешно.\nPostgreSQL 18.6 on x86_64-windows",
                "Проверка подключения", DialogType.Info),
            Path.Combine(directory, "03_dialog_connection.png"));
        CaptureWindow(new Dialog("Вы ввели неверный логин или пароль. Пожалуйста, проверьте ещё раз введённые данные.",
                "Ошибка авторизации", DialogType.Error),
            Path.Combine(directory, "04_dialog_error.png"));
        CaptureWindow(new Dialog("Удалить выбранную запись из базы данных?",
                "Подтверждение действия", DialogType.Question),
            Path.Combine(directory, "10_dialog_question.png"));

        CaptureMain(admin, 1, 0, Path.Combine(directory, "05_main_admin_products_cards.png"));
        CaptureMain(admin, 1, 1, Path.Combine(directory, "06_main_admin_products_table.png"));
        CaptureMain(admin, 2, 0, Path.Combine(directory, "07_main_admin_sales.png"));
        CaptureMain(admin, 3, 0, Path.Combine(directory, "08_main_admin_clients.png"));
        CaptureMain(admin, 0, 0, Path.Combine(directory, "11_dashboard.png"));
        CaptureMain(admin, 6, 0, Path.Combine(directory, "12_users.png"));
        CaptureMain(admin, 5, 0, Path.Combine(directory, "13_references.png"));
        CaptureMain(admin, 7, 0, Path.Combine(directory, "14_reports.png"));
        CaptureMain(manager, 1, 0, Path.Combine(directory, "09_main_manager.png"));

        var product = Database.GetProducts().FirstOrDefault();
        if (product != null)
            CaptureWindow(new ProductEditWindow(product) { Width = 640 }, Path.Combine(directory, "15_product_edit.png"));
        var sale = Database.GetSales().FirstOrDefault();
        if (sale != null)
            CaptureWindow(new SaleEditWindow(sale) { Width = 820, Height = 720 },
                Path.Combine(directory, "16_sale_edit.png"));
        var client = Database.GetClients().FirstOrDefault();
        if (client != null)
            CaptureWindow(new ClientEditWindow(client) { Width = 560 }, Path.Combine(directory, "17_client_edit.png"));
        var user = Database.GetUsers().FirstOrDefault();
        if (user != null)
            CaptureWindow(new UserEditWindow(user) { Width = 560 }, Path.Combine(directory, "18_user_edit.png"));
    }

    private static void CaptureMain(AppUser account, int pageIndex, int tabIndex, string path)
    {
        var window = new MainWindow(account) { Width = 1440, Height = 880 };
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = 20;
        window.Top = 20;
        window.ShowInTaskbar = false;
        window.Show();

        if (pageIndex < window.ViewModel.Pages.Count)
            window.ViewModel.SelectedPage = window.ViewModel.Pages[pageIndex];

        DoEvents();
        Thread.Sleep(800);
        DoEvents();

        if (tabIndex > 0)
        {
            var tabControl = FindChild<TabControl>(window);
            if (tabControl != null) tabControl.SelectedIndex = tabIndex;
        }

        DoEvents();
        window.UpdateLayout();
        DoEvents();
        Thread.Sleep(200);

        using var bitmap = Capture(window);
        bitmap.Save(path, ImageFormat.Png);
        window.Close();
        DoEvents();
    }

    private static void CaptureWindow(Window window, string path)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = 20;
        window.Top = 20;
        window.ShowInTaskbar = false;
        window.Show();
        DoEvents();
        Thread.Sleep(600);
        DoEvents();
        window.UpdateLayout();
        DoEvents();
        using var bitmap = Capture(window);
        bitmap.Save(path, ImageFormat.Png);
        window.Close();
        DoEvents();
    }

    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;
            var nested = FindChild<T>(child);
            if (nested != null) return nested;
        }
        return null;
    }

    private static Bitmap Capture(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        GetWindowRect(handle, out var rect);
        var bitmap = new Bitmap(rect.Right - rect.Left, rect.Bottom - rect.Top);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            var hdc = graphics.GetHdc();
            PrintWindow(handle, hdc, 2);
            graphics.ReleaseHdc(hdc);
        }
        return bitmap;
    }

    private static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
}
