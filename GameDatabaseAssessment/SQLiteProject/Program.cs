using System;
using Microsoft.Data.Sqlite;

internal static class Program
{
    private static void Main()
    {
        ShopDatabase shop = new ShopDatabase("GameShop.db");
        Console.WriteLine("SQLite 상점 평가 시작 프로젝트 (GameShop.db)");

        while (true)
        {
            Console.WriteLine("\n0 연결 | 1 초기화 | 2 아이템 CRUD | 3 정상 구매 | 4 골드 부족");
            Console.WriteLine("5 인벤토리 행 없음 | 6 현재 값 | 7 가격 45 | 8 플레이어 2 | q 종료");
            Console.Write("선택: ");
            string? choice = Console.ReadLine()?.Trim();
            if (choice == null || choice.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                switch (choice)
                {
                    case "0":
                        shop.CheckConnection();
                        break;
                    case "1":
                        shop.Reset();
                        shop.PrintPotionState();
                        break;
                    case "2":
                        shop.Reset();
                        shop.RunItemCrud();
                        break;
                    case "3":
                    case "4":
                    case "5":
                    case "7":
                    case "8":
                        RunPurchaseScenario(shop, choice);
                        break;
                    case "6":
                        shop.PrintPotionState();
                        break;
                    default:
                        Console.WriteLine("메뉴 번호를 다시 입력하세요.");
                        break;
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("실행 오류: " + error.Message);
            }
        }
    }

    private static void RunPurchaseScenario(ShopDatabase shop, string choice)
    {
        shop.Reset();
        int playerId = choice == "8" ? 2 : 1;

        // 고정된 테스트 조건만 바꿉니다. 구매 코드는 아래 PurchasePotion()에 작성합니다.
        if (choice == "4" || choice == "5" || choice == "7")
        {
            using (SqliteConnection connection = shop.OpenConnection())
            using (SqliteCommand setup = connection.CreateCommand())
            {
                if (choice == "4")
                {
                    setup.CommandText = "UPDATE Player SET Gold = 10 WHERE PlayerId = 1;";
                }
                else if (choice == "5")
                {
                    setup.CommandText = "DELETE FROM Inventory WHERE PlayerId = 1 AND ItemId = 1;";
                }
                else
                {
                    setup.CommandText = "UPDATE Item SET Price = 45 WHERE ItemId = 1;";
                }
                setup.ExecuteNonQuery();
            }
        }

        Console.WriteLine("구매 전:");
        shop.PrintPotionState(playerId);
        try
        {
            bool purchased = shop.PurchasePotion(playerId, 1);
            Console.WriteLine(purchased ? "구매 성공" : "구매 실패");
        }
        catch (Exception error)
        {
            Console.WriteLine("구매 중 오류: " + error.Message);
        }
        Console.WriteLine("구매 후:");
        shop.PrintPotionState(playerId);
    }
}

internal sealed class ShopDatabase
{
    private readonly string _path;

    public ShopDatabase(string path)
    {
        _path = path;
    }

    public SqliteConnection OpenConnection()
    {
        SqliteConnection connection = new SqliteConnection("Data Source=" + _path);
        try
        {
            connection.Open();
            using (SqliteCommand pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    public void CheckConnection()
    {
        using (SqliteConnection connection = OpenConnection())
        {
            Console.WriteLine("SQLite 연결 성공");
        }
    }

    public void Reset()
    {
        using (SqliteConnection connection = OpenConnection())
        {
            CreateSchema(connection);
            using (SqliteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    using (SqliteCommand seed = connection.CreateCommand())
                    {
                        seed.Transaction = transaction;
                        seed.CommandText = @"
DELETE FROM Inventory;
DELETE FROM Item;
DELETE FROM Player;
INSERT INTO Player (PlayerId, Name, Gold)
VALUES (1, '민지', 100), (2, '준호', 50);
INSERT INTO Item (ItemId, Name, Price) VALUES (1, 'Potion', 30);
INSERT INTO Inventory (PlayerId, ItemId, Quantity)
VALUES (1, 1, 0), (2, 1, 2);";
                        seed.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            Console.WriteLine("초기 상태를 만들었습니다.");
        }
    }

    public void CreateSchema(SqliteConnection connection)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = @"
    CREATE TABLE IF NOT EXISTS Player (
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
            command.ExecuteNonQuery();
        }
    }

    public void RunItemCrud()
    {
        using (SqliteConnection connection = OpenConnection())
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText =
                "INSERT INTO Item (ItemId, Name, Price) VALUES (2, 'Shield', 50);";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT Price FROM Item WHERE ItemId = 2;";
            Console.WriteLine("등록한 Shield 가격: " + command.ExecuteScalar());

            command.CommandText = "UPDATE Item SET Price = 55 WHERE ItemId = 2;";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT Price FROM Item WHERE ItemId = 2;";
            Console.WriteLine("수정한 Shield 가격: " + command.ExecuteScalar());

            command.CommandText = "DELETE FROM Item WHERE ItemId = 2;";
            command.ExecuteNonQuery();
            Console.WriteLine("Shield를 삭제했습니다.");
        }
    }

    public bool PurchasePotion(int playerId, int itemId)
    {
        using (SqliteConnection connection = OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                object priceValue;
                using (SqliteCommand priceCommand = connection.CreateCommand())
                {
                    priceCommand.Transaction = transaction;
                    priceCommand.CommandText =
                        "SELECT Price FROM Item WHERE ItemId = $itemId;";
                    priceCommand.Parameters.AddWithValue("$itemId", itemId);
                    priceValue = priceCommand.ExecuteScalar();
                }

                if (priceValue == null)
                {
                    transaction.Rollback();
                    return false;
                }
                long price = Convert.ToInt64(priceValue);

                int goldRows;
                using (SqliteCommand goldCommand = connection.CreateCommand())
                {
                    goldCommand.Transaction = transaction;
                    goldCommand.CommandText = @"
    UPDATE Player SET Gold = Gold - $price
    WHERE PlayerId = $playerId AND Gold >= $price;";
                    goldCommand.Parameters.AddWithValue("$price", price);
                    goldCommand.Parameters.AddWithValue("$playerId", playerId);
                    goldRows = goldCommand.ExecuteNonQuery();
                }
                if (goldRows != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                int inventoryRows;
                using (SqliteCommand inventoryCommand = connection.CreateCommand())
                {
                    inventoryCommand.Transaction = transaction;
                    inventoryCommand.CommandText = @"
    UPDATE Inventory SET Quantity = Quantity + 1
    WHERE PlayerId = $playerId AND ItemId = $itemId;";
                    inventoryCommand.Parameters.AddWithValue("$playerId", playerId);
                    inventoryCommand.Parameters.AddWithValue("$itemId", itemId);
                    inventoryRows = inventoryCommand.ExecuteNonQuery();
                }
                if (inventoryRows != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public void PrintPotionState(int playerId = 1)
    {
        using (SqliteConnection connection = OpenConnection())
        {
            object? gold;
            using (SqliteCommand goldCommand = connection.CreateCommand())
            {
                goldCommand.CommandText = "SELECT Gold FROM Player WHERE PlayerId = $playerId;";
                goldCommand.Parameters.AddWithValue("$playerId", playerId);

                // 쿼리 결과를 ExecuteScalar()로 가져옵니다. 결과가 없으면 null이 반환됩니다.
                gold = goldCommand.ExecuteScalar();
            }
            if (gold == null)
            {
                Console.WriteLine("플레이어 " + playerId + "이(가) 없습니다.");
                return;
            }

            object? quantity;
            using (SqliteCommand quantityCommand = connection.CreateCommand())
            {
                quantityCommand.CommandText =
                    "SELECT Quantity FROM Inventory WHERE PlayerId = $playerId AND ItemId = 1;";
                quantityCommand.Parameters.AddWithValue("$playerId", playerId);
                
                // 쿼리 결과를 ExecuteScalar()로 가져옵니다. 결과가 없으면 null이 반환됩니다.
                quantity = quantityCommand.ExecuteScalar();
            }
            Console.WriteLine("플레이어 " + playerId + ": 골드 " + gold
                + ", 포션 " + (quantity == null ? "행 없음" : quantity.ToString()));
        }
    }
}