using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int> FulfillOrder(FulfillOrderDTO order)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = connection.BeginTransaction();
        command.Transaction = transaction as SqlTransaction;
        command.CommandType = CommandType.Text;
        
        try
        {
            if (order.Amount < 1)
            {
                throw new Exception("Order Amount must be greater than 0.");
            }
            
            command.CommandText = @"Select * from Product where IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", order.IdProduct);
            
            Decimal productPrice;
            await using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    productPrice = reader.GetDecimal(reader.GetOrdinal("Price"));
                }
                else
                {
                    throw new Exception("Product not found");
                }

            }
            
            
            command.Parameters.Clear();
            command.CommandText = "Select count(1) from WareHouse where IdWareHouse = @IdWareHouse";
            command.Parameters.AddWithValue("@IdWareHouse", order.IdWareHouse);
            
            if ((int) await command.ExecuteScalarAsync() < 1)
            {
                throw new Exception("Warehouse not found");
            }
            command.Parameters.Clear();
            
            command.CommandText = @"Select * from [Order] where IdProduct = @IdProduct and Amount = @Amount and CreatedAt < @CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", order.IdProduct);
            command.Parameters.AddWithValue("@Amount", order.Amount);
            command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

            int idOrder;
            await using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    idOrder = reader.GetInt32(reader.GetOrdinal("IdOrder"));
                }
                else
                {
                    throw new Exception("Order not found");
                }

            }
            
            command.Parameters.Clear();
            command.CommandText = @"SELECT count(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            if ((int)await command.ExecuteScalarAsync() != 0)
            {
                throw new Exception("Order already fulfilled");
            }
            
            command.Parameters.Clear();
            command.CommandText = @"UPDATE [Order] Set FulfilledAt = @CurrentTime WHERE IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@CurrentTime", DateTime.Now);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.ExecuteNonQuery();
            
            
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO Product_Warehouse (IdWareHouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
                                    VALUES (@IdWareHouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)";
            command.Parameters.AddWithValue("@IdWareHouse", order.IdWareHouse);
            command.Parameters.AddWithValue("@IdProduct", order.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", order.Amount);
            command.Parameters.AddWithValue("@Price", order.Amount*productPrice);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            
            command.ExecuteNonQuery();
            
            command.Parameters.Clear();
            command.CommandText = @"SELECT TOP(1) IdProductWarehouse FROM Product_Warehouse ORDER BY IdProductWarehouse desc";
            var idProductWarehouse = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return idProductWarehouse;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    

    public async Task<Decimal> FulfillOrderProcedure(FulfillOrderDTO order)  
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@IdProduct", order.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", order.IdWareHouse);
        command.Parameters.AddWithValue("@Amount", order.Amount);
        command.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);

        object? id;
        try
        {
            id = await command.ExecuteScalarAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return Decimal.Parse(id?.ToString());
    }
    
}