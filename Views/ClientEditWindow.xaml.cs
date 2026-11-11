using System.Windows;
using HouseholdGoodsIS.Data;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Views;

public partial class ClientEditWindow : Window
{
    private readonly Client _client;

    public ClientEditWindow(Client? client)
    {
        InitializeComponent();
        _client = client ?? new Client();
        TitleText.Text = client == null ? "Добавление клиента" : "Редактирование клиента";
        if (client == null) return;
        LastNameBox.Text = _client.LastName;
        FirstNameBox.Text = _client.FirstName;
        MiddleNameBox.Text = _client.MiddleName ?? string.Empty;
        PhoneBox.Text = _client.Phone;
        EmailBox.Text = _client.Email ?? string.Empty;
        CityBox.Text = _client.City ?? string.Empty;
        AddressBox.Text = _client.Address ?? string.Empty;
        DiscountBox.Text = _client.DiscountCard ?? string.Empty;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(LastNameBox.Text) || string.IsNullOrWhiteSpace(FirstNameBox.Text)
            || string.IsNullOrWhiteSpace(PhoneBox.Text))
        {
            ErrorText.Text = "Заполните фамилию, имя и телефон.";
            return;
        }
        _client.LastName = LastNameBox.Text.Trim();
        _client.FirstName = FirstNameBox.Text.Trim();
        _client.MiddleName = string.IsNullOrWhiteSpace(MiddleNameBox.Text) ? null : MiddleNameBox.Text.Trim();
        _client.Phone = PhoneBox.Text.Trim();
        _client.Email = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text.Trim();
        _client.City = string.IsNullOrWhiteSpace(CityBox.Text) ? null : CityBox.Text.Trim();
        _client.Address = string.IsNullOrWhiteSpace(AddressBox.Text) ? null : AddressBox.Text.Trim();
        _client.DiscountCard = string.IsNullOrWhiteSpace(DiscountBox.Text) ? null : DiscountBox.Text.Trim();
        try
        {
            Database.SaveClient(_client);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ErrorText.Text = "Не удалось сохранить: " + ex.Message;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
