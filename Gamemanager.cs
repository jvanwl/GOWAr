// Add to GameManager.cs Start()
void Start() {
    // Example: spawn one player and one AI unit
    HexTile playerStart = hexGrid.GetTileAt(0, 0);
    HexTile aiStart = hexGrid.GetTileAt(hexGrid.width - 1, hexGrid.height - 1);

    var playerUnit = Instantiate(Resources.Load<Unit>("Unit_Infantry"), playerStart.transform.position, Quaternion.identity);
    playerUnit.Init(factions[0], playerStart);

    var aiUnit = Instantiate(Resources.Load<Unit>("Unit_Infantry"), aiStart.transform.position, Quaternion.identity);
    aiUnit.Init(factions[1], aiStart);
}