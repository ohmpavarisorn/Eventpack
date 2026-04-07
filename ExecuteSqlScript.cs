using MySqlConnector;

namespace EventPack.Utilities;

public class DatabaseInitializer
{
    public static async Task ExecuteSqlScriptAsync()
    {
        var connectionString = "Server=localhost;Database=csi402project;User=root;Password=Pavarisorn239685;";
        
        // First, try to run the migration script to fix foreign keys
        try
        {
            var fixScript = File.ReadAllText("FixDatabase.sql");
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var statements = fixScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var statement in statements)
                {
                    var trimmed = statement.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    
                    using (var command = new MySqlCommand(trimmed, connection))
                    {
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"✓ Migration: {trimmed.Substring(0, Math.Min(60, trimmed.Length))}...");
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("already exists") && !ex.Message.Contains("Cannot delete") && !ex.Message.Contains("reference"))
                            {
                                Console.WriteLine($"✗ Migration Error: {ex.Message}");
                            }
                        }
                    }
                }
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Warning: Could not run migration: {ex.Message}");
        }
        
        // Then run the main creation script
        var sqlScript = File.ReadAllText("CreateRestaurantTables.sql");

        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();
            
            // Split by semicolon and execute each statement
            var statements = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var statement in statements)
            {
                var trimmed = statement.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                
                using (var command = new MySqlCommand(trimmed, connection))
                {
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"✓ Executed: {trimmed.Substring(0, Math.Min(50, trimmed.Length))}...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Error: {ex.Message}");
                    }
                }
            }

            // Add missing columns to foodpackages table
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "CategoryId", "VARCHAR(10) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NULL");
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "MaxGuests", "INT NULL");
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "IconEmoji", "VARCHAR(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL");
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "IsPopular", "TINYINT(1) NOT NULL DEFAULT 0");
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "IsActive", "TINYINT(1) NOT NULL DEFAULT 1");
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "CreatedDate", "DATETIME NULL");
            await AddColumnIfNotExistsAsync(connection, "foodpackages", "MinGuests", "INT NULL");

            // Add missing columns to bookings table
            // Skip MenuItemsJson and CreatedDate as they should be created in FixDatabase.sql
            
            await connection.CloseAsync();
        }

        Console.WriteLine("\n✅ Database tables created successfully!");
    }

    private static async Task AddColumnIfNotExistsAsync(MySqlConnection connection, string tableName, string columnName, string columnDefinition)
    {
        try
        {
            // Check if column exists
            var checkQuery = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'csi402project' AND TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";
            
            using (var command = new MySqlCommand(checkQuery, connection))
            {
                var result = await command.ExecuteScalarAsync();
                if (result != null && (long)result > 0)
                {
                    Console.WriteLine($"  ℹ Column {columnName} already exists in {tableName}");
                    return;
                }
            }

            // Add the column
            var alterQuery = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition}";
            using (var command = new MySqlCommand(alterQuery, connection))
            {
                await command.ExecuteNonQueryAsync();
                Console.WriteLine($"  ✓ Added column {columnName} to {tableName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ✗ Error adding column {columnName} to {tableName}: {ex.Message}");
        }
    }
}

