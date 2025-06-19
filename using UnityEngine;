using UnityEngine;

public class PlayerController : MonoBehaviour {
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) {
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile != null) {
                    if (tile.currentUnit != null && tile.currentUnit.faction.name == "Player") {
                        GameManager.Instance.selectedUnit = tile.currentUnit;
                    } else if (GameManager.Instance.selectedUnit != null) {
                        GameManager.Instance.MoveSelectedUnit(tile);
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            GameManager.Instance.EndTurn();
        }
    }
}