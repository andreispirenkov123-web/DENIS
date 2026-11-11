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

        var authorizationWindow = new AuthorizationWindow();
        MainWindow = authorizationWindow;
        authorizationWindow.Show();
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
