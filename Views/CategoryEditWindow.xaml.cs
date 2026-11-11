using System.Windows;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class CategoryEditWindow : Window
{
    private readonly Category _category;

    public CategoryEditWindow(Category? category)
    {
        InitializeComponent();
        _category = category ?? new Category();
        TitleText.Text = category == null ? "Добавление категории" : "Редактирование категории";
        if (category == null) return;
        NameBox.Text = _category.Name;
        DescriptionBox.Text = _category.Description ?? string.Empty;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            ErrorText.Text = "Укажите название категории.";
            return;
        }
        _category.Name = NameBox.Text.Trim();
        _category.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
        try
        {
            Database.SaveCategory(_category);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
