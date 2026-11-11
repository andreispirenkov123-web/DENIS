namespace HouseholdGoodsIS.Services;

public sealed class ServiceContainer
{
    public static ServiceContainer Instance { get; } = new();
    private readonly Dictionary<Type, object> _services = new();
    private ServiceContainer() { }
    public void Add<T>(T service) where T : class => _services[typeof(T)] = service;
    public T Get<T>() where T : class =>
        _services.TryGetValue(typeof(T), out var service)
            ? (T)service
            : throw new InvalidOperationException($"Сервис {typeof(T).Name} не зарегистрирован");
}
