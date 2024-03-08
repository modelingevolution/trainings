using Microsoft.EntityFrameworkCore;

namespace TrainTicketReservation.Infrastructure;

public static class DbContextExtensions
{
    public static async Task<bool> TableExistsAsync(this DbContext context, string tableName)
    {
        // Create a raw SQL query to check the existence of the table.
            var query = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = @p0";

        // Create a command (ensuring disposal)
        await using var command = context.Database.GetDbConnection().CreateCommand();
        // Set the command text and parameters
        command.CommandText = query;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@p0";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        // Ensure the connection is open
        if (command.Connection.State != System.Data.ConnectionState.Open) 
            await command.Connection.OpenAsync();

        // Execute the command and convert the result
        await using var result = await command.ExecuteReaderAsync();
        return result.HasRows; // True if the table exists
    }

    public static async Task<bool> RecreateIfTableNamedChanged(this DbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        foreach (var t in context.Model.GetRelationalModel().Tables)
        {
            if (!await context.TableExistsAsync(t.Name))
            {
                var script = context.Database.GenerateCreateScript().Split("GO");
                await using var tr = await context.Database.BeginTransactionAsync();
                foreach(var i in script)
                    await context.Database.ExecuteSqlRawAsync(i);
                await tr.CommitAsync();
                return true;
            }
        }

        return false;
    }
}