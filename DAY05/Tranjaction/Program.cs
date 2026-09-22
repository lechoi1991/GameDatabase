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
            int quantity = 3;

            if (quantity <= 0)
            {
                Console.WriteLine("구매 수량은 1개 이상이어야 합니다.");
                return;
            }

            int totalPrice = price * quantity;

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
                            UPDATE Player
                            SET Gold = Gold - $totalPrice
                            WHERE PlayerId = $playerId
                             AND Gold >= $totalPrice;
                            ";
                            spendGold.Parameters.AddWithValue("$totalPrice", totalPrice);
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
                            VALUES ($playerId, $itemId, $quantity)
                            ON CONFLICT (PlayerId, ItemId)
                            DO UPDATE SET Quantity = Quantity + $quantity;
                            ";
                            
                            additem.Parameters.AddWithValue("$playerId", playerId);
                            additem.Parameters.AddWithValue("$itemId", itemId);
                            additem.Parameters.AddWithValue("$quantity", quantity);

                            if (additem.ExecuteNonQuery() != 1)
                            {
                                throw new InvalidOperationException("아이템 추가에 실패했습니다.");
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine($"{quantity}개 구매를 완료했습니다.");
                        Console.WriteLine($"총 {totalPrice} 골드를 사용했습니다.");
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