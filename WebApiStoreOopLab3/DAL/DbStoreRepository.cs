using Microsoft.EntityFrameworkCore;
using WebApiStoreOopLab3.Data;
using WebApiStoreOopLab3.Models;

namespace WebApiStoreOopLab3.DAL;

public class DbStoreRepository : IStoreRepository
{
    private readonly ApplicationDbContext _context;

    public DbStoreRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddStoreAsync(Store store)
    {
        _context.Stores.Add(store);
        await _context.SaveChangesAsync();
    }

    public async Task AddProductAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task AddInventoryAsync(Store store, Product product, int quantity, decimal price)
    {
        var inventoryItem = await _context.Inventory
            .FirstOrDefaultAsync(i => i.StoreCode == store.Code && i.ProductName == product.Name);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                StoreCode = store.Code,
                ProductName = product.Name,
                Quantity = quantity,
                Price = price
            };
            _context.Inventory.Add(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += quantity;
            if (price > 0)
            {
                inventoryItem.Price = price;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Store> FindCheapestStoreForProductAsync(string productName)
    {
        var cheapestStoreCode = await _context.Inventory
            .Where(i => i.ProductName == productName && i.Quantity > 0)
            .OrderBy(i => (double)i.Price)
            .Select(i => i.StoreCode)
            .FirstOrDefaultAsync();

        return await _context.Stores.FindAsync(cheapestStoreCode);
    }

    public async Task<Dictionary<string, int>> GetAffordableProductsAsync(Store store, decimal budget)
    {
        var affordableProducts = await _context.Inventory
            .Where(i => i.StoreCode == store.Code && i.Quantity > 0 && i.Price <= budget)
            .OrderBy(i => (double)i.Price)
            .Select(i => new 
            {
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity
            })
            .ToListAsync();
        
        var result = new Dictionary<string, int>();
        
        foreach (var item in affordableProducts)
        {
            int maxQuantity = (int)(budget / item.Price);
            if (item.Quantity < maxQuantity) maxQuantity = item.Quantity;


            result[item.ProductName] = maxQuantity;
        }
        
        return result;
    }
    

    public async Task<decimal?> CalculatePurchaseCostAsync(Store store, Dictionary<Product, int> productQuantities)
    {
        decimal totalCost = 0;

        foreach (var (product, quantity) in productQuantities)
        {
            var inventoryItem = await _context.Inventory
                .FirstOrDefaultAsync(i => i.StoreCode == store.Code && i.ProductName == product.Name);

            if (inventoryItem == null || inventoryItem.Quantity < quantity)
            {
                return null;
            }

            totalCost += inventoryItem.Price * quantity;
        }

        return totalCost;
    }

    public async Task<Store> FindBestStoreForBulkPurchaseAsync(Dictionary<Product, int> productQuantities)
    {
        var stores = await _context.Stores.ToListAsync();
        Store bestStore = null;
        decimal lowestTotalCost = decimal.MaxValue;

        foreach (var store in stores)
        {
            decimal? cost = await CalculatePurchaseCostAsync(store, productQuantities);
            if (cost.HasValue && cost.Value < lowestTotalCost)
            {
                bestStore = store;
                lowestTotalCost = cost.Value;
            }
        }

        return bestStore;
    }

    public async Task<Store> FindStoreByCodeAsync(int storeCode)
    {
        return await _context.Stores.FindAsync(storeCode);
    }

    public async Task<Product> FindProductByNameAsync(string productName)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Name == productName);
    }
}
