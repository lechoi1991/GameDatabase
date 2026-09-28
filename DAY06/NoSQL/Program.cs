using System;
using LiteDB;

public class GameLog
{
    public int Id { get; set; }
    public string EventType { get; set; } = "";
    public int PlayerId { get; set; }
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string ItemId { get; set; } = "";
}

internal class Program
{
   private static void Main(string[] args)
    {
        using (LiteDatabase db = new LiteDatabase("GameLogs.db"))
        {
            ILiteCollection<GameLog> logs = db.GetCollection<GameLog>("logs");

            GameLog logEntry = new GameLog
            {
                EventType = "PlayerJoined",
                PlayerId = 1,
                Message = "Player 1 has joined the game",
                Timestamp = DateTime.Now
            };
            logs.Insert(logEntry);

            GameLog logPurchase = new GameLog
            {
                EventType = "PurchaseFailed",
                PlayerId = 1,
                ItemId = "Potion",
                Message = "골드가 부족합니다.",
                Timestamp = DateTime.Now
            };
            logs.Insert(logPurchase);

            foreach (var log in logs.FindAll())
            {
                Console.WriteLine($"<Id:{log.Id}>[{log.Timestamp}]{log.EventType} - Player {log.PlayerId} - Message {log.ItemId}: {log.Message}");
            }
        }
    } 
}