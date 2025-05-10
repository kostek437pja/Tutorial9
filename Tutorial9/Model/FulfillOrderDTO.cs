namespace Tutorial9.Model;

public class FulfillOrderDTO
{
    public int IdWareHouse { get; set; }
    public int IdProduct { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}