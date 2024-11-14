namespace WebApiStoreOopLab3.Models;

public class InventoryItem
{
    public Store Store { get; set; }
    public Product Product { get; set; }
    
    public int StoreCode { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}