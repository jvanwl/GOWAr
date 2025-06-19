public class HexTile : MonoBehaviour {
    public int x, y;
    public BiomeType biome;
    public Faction owner;
    public Unit currentUnit;
    public int defenseBonus;

    public void Init(int x, int y, BiomeType biome) {
        this.x = x;
        this.y = y;
        this.biome = biome;
        defenseBonus = (biome == BiomeType.Mountain) ? 2 : 0;
        GetComponent<SpriteRenderer>().color = GetBiomeColor();
    }

    Color GetBiomeColor() {
        switch(biome) {
            case BiomeType.Forest: return new Color(0.1f, 0.6f, 0.1f);
            case BiomeType.Mountain: return Color.gray;
            case BiomeType.Desert: return new Color(0.9f, 0.8f, 0.5f);
            default: return new Color(0.6f, 0.9f, 0.6f);
        }
    }

    void OnMouseDown() {
        if (GameManager.Instance.selectedUnit != null) {
            GameManager.Instance.MoveSelectedUnit(this);
        }
    }
}
