using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IDbService
{
    Task<Decimal> FulfillOrderProcedure(FulfillOrderDTO order);
    Task<int> FulfillOrder(FulfillOrderDTO order);
}