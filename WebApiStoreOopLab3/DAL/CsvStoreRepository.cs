using System.Globalization;
using WebApiStoreOopLab3.Models;

namespace WebApiStoreOopLab3.DAL;

public class CsvStoreRepository : IStoreRepository
{
    private readonly string _storeFilePath;
    private readonly string _inventoryFilePath;

    public CsvStoreRepository(string storeFilePath, string inventoryFilePath)
    {
        _storeFilePath = storeFilePath;
        _inventoryFilePath = inventoryFilePath;

        EnsureFileExists(_storeFilePath, "StoreCode,StoreName,Address");
        EnsureFileExists(_inventoryFilePath, "StoreCode,ProductName,Quantity,Price");
    }

    private void EnsureFileExists(string filePath, string header)
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        if (!File.Exists(filePath)) File.WriteAllText(filePath, header + "\n");
    }

    public async Task AddStoreAsync(Store store)
    {
        using (var writer = new StreamWriter(_storeFilePath, append: true))
        {
            await writer.WriteLineAsync($"{store.Code},{store.Name},{store.Address}");
        }
    }

    public async Task AddProductAsync(Product product)
    {
        await Task.CompletedTask; // не стал создавать для продуктов отдельный файл с одним столбцом
    }

    public async Task AddInventoryAsync(Store store, Product product, int quantity, decimal price)
    {
        var lines = await File.ReadAllLinesAsync(_inventoryFilePath);
        var inventoryLines = lines.Skip(1).ToList();

        var existingLineIndex = inventoryLines.FindIndex(line =>
        {
            var data = line.Split(',');
            return data[0] == store.Code.ToString() && data[1] == product.Name;
        });

        if (existingLineIndex >= 0)
        {
            var data = inventoryLines[existingLineIndex].Split(',');
            var currentQuantity = int.Parse(data[2]);
            var currentPrice = decimal.Parse(data[3], CultureInfo.InvariantCulture);

            var newQuantity = currentQuantity + quantity;
            var newPrice = price > 0 ? price : currentPrice;

            inventoryLines[existingLineIndex] = $"{store.Code},{product.Name},{newQuantity},{newPrice.ToString(CultureInfo.InvariantCulture)}";
        }
        else
        {
            var newLine = $"{store.Code},{product.Name},{quantity},{price.ToString(CultureInfo.InvariantCulture)}";
            inventoryLines.Add(newLine);
        }

        await File.WriteAllLinesAsync(_inventoryFilePath, new[] { "StoreCode,ProductName,Quantity,Price" }.Concat(inventoryLines));
    }


    public async Task<Store> FindCheapestStoreForProductAsync(string productName)
    {
        var lines = (await File.ReadAllLinesAsync(_inventoryFilePath)).Skip(1);
        decimal? lowestPrice = null;
        string storeCode = null;

        foreach (var line in lines)
        {
            var data = line.Split(',');
            if (data[1] == productName && int.Parse(data[2]) > 0)
            {
                var price = decimal.Parse(data[3], CultureInfo.InvariantCulture);
                if (!lowestPrice.HasValue || price < lowestPrice.Value)
                {
                    lowestPrice = price;
                    storeCode = data[0];
                }
            }
        }

        return storeCode != null ? await FindStoreByCodeAsync(int.Parse(storeCode)) : null;
    }

    public async Task<Dictionary<string, int>> GetAffordableProductsAsync(Store store, decimal budget)
    {
        var lines = (await File.ReadAllLinesAsync(_inventoryFilePath)).Skip(1); // заголовок
        var result = new Dictionary<string, int>();

        var affordableItems = lines
            .Select(line => line.Split(','))
            .Where(data => int.Parse(data[0]) == store.Code && int.Parse(data[2]) > 0)
            .OrderBy(data => decimal.Parse(data[3], CultureInfo.InvariantCulture));

        foreach (var item in affordableItems)
        {
            var productName = item[1];
            var quantity = int.Parse(item[2]);
            var price = decimal.Parse(item[3], CultureInfo.InvariantCulture);
            int maxQuantity = (int)(budget / price);
            if (quantity < maxQuantity) maxQuantity = quantity;

            result[productName] = maxQuantity;
        }

        return result;
    }

    public async Task<decimal?> CalculatePurchaseCostAsync(Store store, Dictionary<Product, int> productQuantities)
    {
        decimal totalCost = 0;
        var lines = (await File.ReadAllLinesAsync(_inventoryFilePath)).Skip(1);

        foreach (var (product, quantity) in productQuantities)
        {
            var item = lines
                .Select(line => line.Split(','))
                .FirstOrDefault(data => int.Parse(data[0]) == store.Code && data[1] == product.Name);

            if (item == null || int.Parse(item[2]) < quantity)
                return null;

            var price = decimal.Parse(item[3], CultureInfo.InvariantCulture);
            totalCost += price * quantity;
        }

        return totalCost;
    }

    public async Task<Store> FindBestStoreForBulkPurchaseAsync(Dictionary<Product, int> productQuantities)
    {
        var lines = (await File.ReadAllLinesAsync(_storeFilePath)).Skip(1);
        var stores = lines.Select(line =>
        {
            var data = line.Split(',');
            return new Store
            {
                Code = int.Parse(data[0]),
                Name = data[1],
                Address = data[2]
            };
        });

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
        var lines = (await File.ReadAllLinesAsync(_storeFilePath)).Skip(1);
        var storeLine = lines.Select(line => line.Split(','))
            .FirstOrDefault(data => int.Parse(data[0]) == storeCode);

        if (storeLine == null) return null;

        return new Store
        {
            Code = storeCode,
            Name = storeLine[1],
            Address = storeLine[2]
        };
    }
    
    public async Task<Product> FindProductByNameAsync(string productName)
    {
        var lines = (await File.ReadAllLinesAsync(_inventoryFilePath)).Skip(1);
        var productLine = lines.Select(line => line.Split(','))
            .FirstOrDefault(data => data[1] == productName);

        if (productLine == null) return null;

        return new Product
        {
            Name = productName
        };
    }
}
