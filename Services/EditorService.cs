using System.Windows;
using HouseholdGoodsIS.Models;
using HouseholdGoodsIS.Views;

namespace HouseholdGoodsIS.Services;

public interface IEditorService
{
    bool EditProduct(Product? product);
    bool EditClient(Client? client);
    bool EditSale(Sale? sale);
    bool EditSupply(Supply? supply);
    bool EditCategory(Category? category);
    bool EditSupplier(Supplier? supplier);
    bool EditUser(AppUser? user);
}

public class EditorService : IEditorService
{
    private static Window Owner => Application.Current.MainWindow;

    public bool EditProduct(Product? product) => new ProductEditWindow(product) { Owner = Owner }.ShowDialog() == true;
    public bool EditClient(Client? client) => new ClientEditWindow(client) { Owner = Owner }.ShowDialog() == true;
    public bool EditSale(Sale? sale) => new SaleEditWindow(sale) { Owner = Owner }.ShowDialog() == true;
    public bool EditSupply(Supply? supply) => new SupplyEditWindow(supply) { Owner = Owner }.ShowDialog() == true;
    public bool EditCategory(Category? category) => new CategoryEditWindow(category) { Owner = Owner }.ShowDialog() == true;
    public bool EditSupplier(Supplier? supplier) => new SupplierEditWindow(supplier) { Owner = Owner }.ShowDialog() == true;
    public bool EditUser(AppUser? user) => new UserEditWindow(user) { Owner = Owner }.ShowDialog() == true;
}
