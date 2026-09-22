using System;
using Microsoft.Data.Sqlite;

namespace Transaction
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int playerId = 1;
            int itemId = 1;
            int price = 30;

            using (SqliteConnection connection = new SqliteConnection("Data Source=test.db"))
            {
                connection.Open();

                using (SqliteCommand pragma = connection.CreateCommand())
                {
                    pragma.CommandText = "PRAGMA foreign_keys = ON;";
                    pragma.ExecuteNonQuery();
                }

                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqliteCommand spendGold = connection.CreateCommand())
                        {
                            spendGold.Transaction = transaction;
                            spendGold.CommandText = @"
                            UPDATE Player SET Gold = Gold - $price
                            WHERE PlayerId = $playerId AND Gold >= $price;
                            ";
                            spendGold.Parameters.AddWithValue("$price", price);
                            spendGold.Parameters.AddWithValue("$playerId", playerId);

                            if (spendGold.ExecuteNonQuery() != 1)
                            {
                                throw new InvalidOperationException("플레이어가 없거나 골드가 부족합니다.");
                            }
                        }

                        using (SqliteCommand additem = connection.CreateCommand())
                        {
                            additem.Transaction = transaction;
                            additem.CommandText = @"
                            INSERT INTO Inventory (PlayerId, ItemId, Quantity)
                            VALUES ($playerId, $itemId, 1)
                            ON CONFLICT (PlayerId, ItemId)
                            DO UPDATE SET Quantity = Quantity + 1;
                            ";
                            
                            additem.Parameters.AddWithValue("$playerId", playerId);
                            additem.Parameters.AddWithValue("$itemId", itemId);

                            if (additem.ExecuteNonQuery() != 1)
                            {
                                throw new InvalidOperationException("인벤토리에 해당 아이템 행이 없습니다.");
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine("구매를 완료했습니다.");
                    }
                    catch (Exception exception)
                    {
                        transaction.Rollback();
                        Console.WriteLine("구매를 취소했습니다: " + exception.Message);
                    }
                }
            }
        }
    }
}