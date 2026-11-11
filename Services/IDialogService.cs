namespace HouseholdGoodsIS.Services;

public enum DialogType { Info, Error, Question }
public enum DialogResult { Ok, Yes, No }

public interface IDialogService
{
    DialogResult ShowDialog(string text, string title, DialogType type);
    bool ShowPasswordDialog(string title, string message, out string? newPassword);
}
