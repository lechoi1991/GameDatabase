SELECT Player.Name, Item.name, Inventory.Quantity
FROM Inventory
JOIN Player ON Inventory.PlayerId = Player.PlayerId
JOIN Item ON Inventory.ItemId = Item.itemId;