using System.Collections.Generic;
using System.IO;
using SQLite;
using TMPro;
using UnityEngine;

public class ItemRow
{
    public int ItemId { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
}

public class GameShopDatabaseReader : MonoBehaviour
{
    [SerializeField] private TMP_Text itemText;

    private void Start()
    {
        string databasePath = Path.Combine(
            Application.streamingAssetsPath,
            "GameShop.sqlite");

        if (!File.Exists(databasePath))
        {
            itemText.text = "GameShop.sqlite is not exist";
            Debug.LogError(
                "SQLite DB 파일을 찾을 수 없습니다: " +
                databasePath);

            return;
        }

        using (SQLiteConnection database =
            new SQLiteConnection(databasePath))
        {
            List<ItemRow> items = database.Query<ItemRow>(
                "SELECT ItemId, Name, Price " +
                "FROM Item " +
                "ORDER BY ItemId");

            if (items.Count == 0)
            {
                itemText.text = "Data is not exist on 'Item' Table";
                Debug.LogWarning(
                    "Item 테이블에 데이터가 없습니다.");

                return;
            }

            ItemRow firstItem = items[0];

            itemText.text =
                firstItem.Name +
                " : " +
                firstItem.Price +
                " Gold";

            Debug.Log(
                "SQLite Item: " +
                itemText.text);
        }
    }
}