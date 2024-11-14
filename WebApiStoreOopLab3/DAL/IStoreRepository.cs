using WebApiStoreOopLab3.Models;

namespace WebApiStoreOopLab3.DAL;

public interface IStoreRepository
{
    Task AddStoreAsync(Store store);
    Task AddProductAsync(Product product);
    Task AddInventoryAsync(Store store, Product product, int quantity, decimal price);
    Task<Store> FindCheapestStoreForProductAsync(string productName);
    Task<Dictionary<string, int>> GetAffordableProductsAsync(Store store, decimal budget);
    Task<decimal?> CalculatePurchaseCostAsync(Store store, Dictionary<Product, int> productQuantities);
    Task<Store> FindBestStoreForBulkPurchaseAsync(Dictionary<Product, int> productQuantities);
    
    Task<Store> FindStoreByCodeAsync(int storeCode);
    Task<Product> FindProductByNameAsync(string productName);
}
