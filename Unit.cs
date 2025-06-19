using System.Collections;

public class Unit : MonoBehaviour {
    public UnitType type;
    public Faction faction;
    public HexTile currentTile;
    public int health = 100;
    public int attack = 10;
    public int defense = 5;
    public int movementRange = 3;

    public void MoveTo(HexTile target) {
        StartCoroutine(MoveAlongPath(GameManager.Instance.pathfinder.FindPath(currentTile, target)));
    }

    IEnumerator MoveAlongPath(List<HexTile> path) {
        foreach (HexTile tile in path) {
            if (tile.currentUnit != null) {
                if (tile.currentUnit.faction != faction) {
                    CombatSystem.ResolveCombat(this, tile.currentUnit);
                    yield break;
                }
                continue;
            }

            transform.position = tile.transform.position;
            currentTile.currentUnit = null;
            currentTile = tile;
            tile.currentUnit = this;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
