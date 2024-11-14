using Microsoft.AspNetCore.Mvc;
using WebApiStoreOopLab3.BLL;

namespace WebApiStoreOopLab3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly StoreService _storeService;

    public StoreController(StoreService storeService)
    {
        _storeService = storeService;
    }
    
    /// <summary>
    /// 1. Добавление нового магазина.
    /// </summary>
    /// <param name="storeDto">Данные нового магазина.</param>
    [HttpPost("AddStore")]
    public async Task<IActionResult> AddStore([FromBody] StoreDto storeDto)
    {
        await _storeService.AddStoreAsync(storeDto.Name, storeDto.Address);
        return Ok("Магазин успешно добавлен.");
    }

    /// <summary>
    /// 2. Добавление нового продукта.
    /// </summary>
    /// <param name="productDto">Данные нового продукта.</param>
    [HttpPost("AddProduct")]
    public async Task<IActionResult> AddProduct([FromBody] ProductDto productDto)
    {
        await _storeService.AddProductAsync(productDto.Name);
        return Ok("Продукт успешно добавлен.");
    }

    /// <summary>
    /// 3. Добавление/обновление количества и цены товара в магазине (завоз партии).
    /// </summary>
    /// <param name="inventoryDto">Данные товара для обновления в инвентаре.</param>
    [HttpPost("AddInventory")]
    public async Task<IActionResult> AddInventory([FromBody] InventoryDto inventoryDto)
    {
        await _storeService.AddInventoryAsync(inventoryDto.StoreCode, inventoryDto.ProductName, inventoryDto.Quantity, inventoryDto.Price);
        return Ok($"Завоз партии в магазин {inventoryDto.StoreCode}: {inventoryDto.ProductName}. Количество: {inventoryDto.Quantity}, Цена: {inventoryDto.Price}");
    }

    /// <summary>
    /// 4. Поиск магазина с минимальной ценой на указанный товар.
    /// </summary>
    /// <param name="productName">Название продукта для поиска.</param>
    [HttpGet("CheapestStoreForProduct")]
    public async Task<IActionResult> GetCheapestStoreForProduct(string productName)
    {
        var store = await _storeService.GetCheapestStoreForProductAsync(productName);
        return store != null ? Ok(store) : NotFound("Магазин с указанным продуктом не найден.");
    }

    /// <summary>
    /// 5. Поиск товаров, которые можно купить в магазине на указанную сумму (Товар - Количество).
    /// </summary>
    /// <param name="storeCode">Код магазина.</param>
    /// <param name="budget">Доступный бюджет для покупки товаров.</param>
    [HttpGet("AffordableProducts")]
    public async Task<IActionResult> GetAffordableProducts(int storeCode, decimal budget)
    {
        var affordableProducts = await _storeService.GetAffordableProductsAsync(storeCode, budget);
        return Ok(affordableProducts);
    }

    /// <summary>
    /// 6. Попкупка партии товаров в магазине и вывод общей стоимости этой покупки.
    /// </summary>
    /// <param name="purchaseDto">Код магазина, названия и количества товаров, которые необходимо купить.</param>
    [HttpPost("PurchaseProducts")]
    public async Task<IActionResult> PurchaseProducts([FromBody] PurchaseDto purchaseDto)
    {
        var totalCost = await _storeService.PurchaseProductsAsync(purchaseDto.StoreCode, purchaseDto.ProductsToBuy);
        return totalCost.HasValue ? Ok($"Общая стоимость покупки: {totalCost.Value}") : BadRequest("Покупка невозможна. Недостаточно товаров.");
    }

    /// <summary>
    /// 7. Поиск магазина, где партия товаров будет стоить дешевле всего.
    /// </summary>
    /// <param name="bulkPurchaseDto">Названия и количества товаров.</param>
    [HttpPost("BestStoreForBulkPurchase")]
    public async Task<IActionResult> FindBestStoreForBulkPurchase([FromBody] BulkPurchaseDto bulkPurchaseDto)
    {
        var store = await _storeService.FindBestStoreForBulkPurchaseAsync(bulkPurchaseDto.ProductsToBuy);
        return store != null ? Ok(store) : NotFound("Подходящий магазин не найден.");
    }
}

public class StoreDto
{
    public string Name { get; set; }
    public string Address { get; set; }
}

public class ProductDto
{
    public string Name { get; set; }
}

public class InventoryDto
{
    public int StoreCode { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class PurchaseDto
{
    public int StoreCode { get; set; }
    public Dictionary<string, int> ProductsToBuy { get; set; }
}

public class BulkPurchaseDto
{
    public Dictionary<string, int> ProductsToBuy { get; set; }
}
