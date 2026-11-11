using HouseholdGoodsIS.Views;

namespace HouseholdGoodsIS.Services;

public class DialogService : IDialogService
{
    public DialogResult ShowDialog(string text, string title, DialogType type)
    {
        var dialog = new Dialog(text, title, type);
        dialog.ShowDialog();
        if (dialog.Result == null) return DialogResult.Ok;
        return dialog.Result == true ? DialogResult.Yes : DialogResult.No;
    }

    public bool ShowPasswordDialog(string title, string message, out string? newPassword)
    {
        var dialog = new ChangePasswordDialog(message) { Owner = System.Windows.Application.Current.MainWindow };
        var result = dialog.ShowDialog();
        newPassword = dialog.NewPassword;
        return result == true;
    }
}
