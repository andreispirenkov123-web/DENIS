using System.Collections.ObjectModel;
using HouseholdGoodsIS.Models;

namespace HouseholdGoodsIS.Services;

public sealed class CartService
{
    public static CartService Instance { get; } = new();
    private CartService() { }

    public ObservableCollection<CartItem> Items { get; } = new();
    public event EventHandler? Changed;

    public int Count => Items.Sum(i => i.Quantity);
    public decimal Total => Items.Sum(i => i.Sum);
    public bool IsEmpty => Items.Count == 0;

    public bool Add(Product product, int quantity = 1)
    {
        if (product.Stock <= 0) return false;
        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
        var current = existing?.Quantity ?? 0;
        if (current + quantity > product.Stock) return false;

        if (existing == null)
            Items.Add(new CartItem { Product = product, Quantity = quantity });
        else
            existing.Quantity += quantity;
        RaiseChanged();
        return true;
    }

    public void Increase(CartItem item)
    {
        if (item.Quantity >= item.Product.Stock) return;
        item.Quantity++;
        RaiseChanged();
    }

    public void Decrease(CartItem item)
    {
        if (item.Quantity <= 1) return;
        item.Quantity--;
        RaiseChanged();
    }

    public void Remove(CartItem item)
    {
        Items.Remove(item);
        RaiseChanged();
    }

    public void Clear()
    {
        Items.Clear();
        RaiseChanged();
    }

    public void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
