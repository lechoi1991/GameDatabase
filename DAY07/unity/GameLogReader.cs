using System.IO;
using System.Linq;
using LiteDB;
using TMPro;
using UnityEngine;

public class GameLog
{
    public int Id { get; set; }
    public string EventType { get; set; } = "";
    public int PlayerId { get; set; }
    public string Message { get; set; } = "";
}

public class GameLogReader : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    private void Start()
    {
        string sourcePath = Path.Combine(
            Application.streamingAssetsPath,
            "GameLogs.db");

        string savePath = Path.Combine(
            Application.persistentDataPath,
            "GameLogs.db");

        if (!File.Exists(savePath))
        {
            File.Copy(sourcePath, savePath);
        }

        using (LiteDatabase database = new LiteDatabase(savePath))
        {
            ILiteCollection<GameLog> logs =
                database.GetCollection<GameLog>("logs");

            var gameLogs = logs
                .Find(x => x.Id > 0)
                .OrderBy(x => x.Id)
                .Take(2)
                .ToList();

            if (gameLogs.Count == 0)
            {
                logText.text = "읽을 로그가 없습니다.";
                return;
            }

            logText.text = "";

            foreach (GameLog log in gameLogs)
            {
                logText.text +=
                    log.EventType +
                    ": " +
                    log.Message +
                    "\n";
            }

            Debug.Log(logText.text);
        }
    }
}