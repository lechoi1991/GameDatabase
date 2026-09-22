using System;
using Microsoft.Data.Sqlite;

namespace GameDatabaseLab
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string connectionString = "Data Source=GameShop.db";
            string createTableSql = @"
            CREATE TABLE IF NOT EXISTS Player(
            PlayerId INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Gold INTEGER NOT NULL CHECK (Gold >= 0)
            );

            CREATE TABLE IF NOT EXISTS Item (
            ItemId INTEGER PRIMARY KEY,
            Name TEXT NOT NULL,
            Price INTEGER NOT NULL CHECK (Price >= 0)
            );

            CREATE TABLE IF NOT EXISTS Inventory (
            PlayerId INTEGER NOT NULL,
            ItemId INTEGER NOT NULL,
            Quantity INTEGER NOT NULL CHECK (Quantity >= 0),
            PRIMARY KEY (PlayerId, ItemId),
            FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId),
            FOREIGN KEY (ItemId) REFERENCES Item(ItemId)
            );";

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    using (SqliteCommand pragma = connection.CreateCommand())
                    {
                        pragma.CommandText = "PRAGMA foreign_keys = ON;";
                        pragma.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = createTableSql;
                        command.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT OR IGNORE INTO Player (PlayerId, Name, Gold)
                        VALUES (1, '민지', 100);
                        ";

                        command.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT OR IGNORE INTO Item (ItemId, Name, Price)
                        VALUES (1, '회복 포션', 30);
                        ";

                        command.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT OR IGNORE INTO Inventory (PlayerId, ItemId, Quantity)
                        VALUES ($playerId, $itemId, $quantity);
                        ";

                        command.Parameters.AddWithValue("$playerId", 1);
                        command.Parameters.AddWithValue("$itemId", 1);
                        command.Parameters.AddWithValue("$quantity", 5);

                        command.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT OR IGNORE INTO Item (ItemId, Name, Price)
                        VALUES ($itemId, $name, $price);
                        ";
                        command.Parameters.AddWithValue("$itemId", 2);
                        command.Parameters.AddWithValue("$name", "철 검");
                        command.Parameters.AddWithValue("$price", 100);

                        command.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        INSERT OR IGNORE INTO Inventory (PlayerId, ItemId, Quantity)
                        VALUES ($playerId, $itemId, $quantity);
                        ";

                        command.Parameters.AddWithValue("$playerId", 1);
                        command.Parameters.AddWithValue("$itemId", 2);
                        command.Parameters.AddWithValue("$quantity", 1);

                        command.ExecuteNonQuery();
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        SELECT Item.Name, Inventory.Quantity
                        FROM Inventory
                        JOIN Item ON Inventory.ItemId = Item.ItemId
                        WHERE Inventory.PlayerId = $playerId;
                        ";

                        command.Parameters.AddWithValue("$playerId", 1);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string itemName = reader.GetString(0);
                                int quantity = reader.GetInt32(1);

                                Console.WriteLine($"{itemName} : {quantity}개");
                            }
                        }
                    }

                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                        SELECT ItemId, Name, Price
                        FROM Item
                        WHERE Price >= $minPrice
                        AND Price <= $maxPrice;
                        ";

                        command.Parameters.AddWithValue("$minPrice", 40);
                        command.Parameters.AddWithValue("$maxPrice", 100);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int itemId = reader.GetInt32(0);
                                string itemName = reader.GetString(1);
                                int price = reader.GetInt32(2);

                                Console.WriteLine(
                                    $"아이템 번호: {itemId}, 이름: {itemName}, 가격: {price}"
                                );
                            }
                        }
                    }
                Console.WriteLine("오류없이 실행 완료했습니다.");
                }
        }
    }
}