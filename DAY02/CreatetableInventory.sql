CREATE TABLE IF NOT EXISTS Inventory (
    PlayerId INTEGER NOT NULL,
    ItemId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL CHECK (Quantity >= 0),
    PRIMARY KEY (PlayerId, ItemId),
    FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId),
    FOREIGN KEY (ItemId) REFERENCES Item(itemId)
);