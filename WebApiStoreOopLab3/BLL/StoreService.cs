using WebApiStoreOopLab3.DAL;
using WebApiStoreOopLab3.Models;

namespace WebApiStoreOopLab3.BLL;

public class StoreService
{
    private readonly IStoreRepository _repository;

    public StoreService(IStoreRepository repository)
    {
        _repository = repository;
    }
    
    public async Task AddStoreAsync(string name, string address)
    {
        var store = new Store
        {
            Code = GenerateUniqueCode(),
            Name = name,
            Address = address
        };

        await _repository.AddStoreAsync(store);
    }
    
    public async Task AddProductAsync(string productName)
    {
        var product = new Product { Name = productName };
        await _repository.AddProductAsync(product);
    }
    
    public async Task AddInventoryAsync(int storeCode, string productName, int quantity, decimal price)
    {
        var store = await _repository.FindStoreByCodeAsync(storeCode);
        if (store == null) throw new ArgumentException("Магазин не найден.");

        var product = await _repository.FindProductByNameAsync(productName);
        if (product == null)
        {
            product = new Product { Name = productName };
            await _repository.AddProductAsync(product);
        }

        await _repository.AddInventoryAsync(store, product, quantity, price);
    }

    public async Task<Store> GetCheapestStoreForProductAsync(string productName)
    {
        return await _repository.FindCheapestStoreForProductAsync(productName);
    }
    
    public async Task<Dictionary<string, int>> GetAffordableProductsAsync(int storeCode, decimal budget)
    {
        var store = await _repository.FindStoreByCodeAsync(storeCode);
        if (store == null) throw new ArgumentException("Магазин не найден.");

        return await _repository.GetAffordableProductsAsync(store, budget);
    }

    public async Task<decimal?> PurchaseProductsAsync(int storeCode, Dictionary<string, int> productsToBuy)
    {
        var store = await _repository.FindStoreByCodeAsync(storeCode);
        if (store == null) throw new ArgumentException("Магазин не найден.");

        var productQuantities = new Dictionary<Product, int>();
        foreach (var (productName, quantity) in productsToBuy)
        {
            var product = await _repository.FindProductByNameAsync(productName);
            if (product == null) throw new ArgumentException($"Продукт '{productName}' не найден.");
            productQuantities[product] = quantity;
        }

        var totalCost = await _repository.CalculatePurchaseCostAsync(store, productQuantities);
        if (totalCost == null) throw new InvalidOperationException("Недостаточно товаров для выполнения покупки.");

        foreach (var (product, quantity) in productQuantities)
        {
            await _repository.AddInventoryAsync(store, product, -quantity, 0); // если price 0, то оставить price прежним
        }

        return totalCost;
    }

    public async Task<Store> FindBestStoreForBulkPurchaseAsync(Dictionary<string, int> productsToBuy)
    {
        var productQuantities = new Dictionary<Product, int>();
        foreach (var (productName, quantity) in productsToBuy)
        {
            var product = await _repository.FindProductByNameAsync(productName);
            if (product == null) throw new ArgumentException($"Продукт '{productName}' не найден.");
            productQuantities[product] = quantity;
        }

        return await _repository.FindBestStoreForBulkPurchaseAsync(productQuantities);
    }

    private int GenerateUniqueCode()
    {
        return new Random().Next(10000, 99999);
    }
}

