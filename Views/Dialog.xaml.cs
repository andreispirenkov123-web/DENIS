using System.Windows;
using System.Windows.Media;
using HouseholdGoodsIS.Services;

namespace HouseholdGoodsIS.Views;

public partial class Dialog : Window
{
    public Dialog(string text, string title, DialogType type)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = text;
        ApplyType(type);
    }

    public bool? Result { get; private set; }

    private void ApplyType(DialogType type)
    {
        switch (type)
        {
            case DialogType.Info:
                IconBorder.Background = (Brush)FindResource("SuccessBrush");
                IconText.Text = "i";
                break;
            case DialogType.Error:
                IconBorder.Background = (Brush)FindResource("ErrorBrush");
                IconText.Text = "!";
                break;
            case DialogType.Question:
                IconBorder.Background = (Brush)FindResource("AccentBrush");
                IconText.Text = "?";
                break;
        }
        if (type == DialogType.Question)
        {
            OkButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            YesButton.Visibility = Visibility.Collapsed;
            NoButton.Visibility = Visibility.Collapsed;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e) { Result = null; Close(); }
    private void Yes_Click(object sender, RoutedEventArgs e) { Result = true; Close(); }
    private void No_Click(object sender, RoutedEventArgs e) { Result = false; Close(); }
}
