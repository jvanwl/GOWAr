using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    public HexGrid hexGrid; // Asigna en el inspector o se busca automáticamente
    public Pathfinder pathfinder; // Asigna en el inspector o se busca automáticamente
    public List<Faction> factions = new List<Faction>();
    public Unit selectedUnit;
    public Transform flagPanel; // Asigna un GameObject vacío en la UI para mostrar banderas
    public GameObject flagImagePrefab; // Prefab con un componente Image
    public AudioSource audioSource;
    public AudioClip[] cultureClips; // Asigna clips por cultura en el inspector

    void Awake() {
        Instance = this;
        if (hexGrid == null) hexGrid = FindObjectOfType<HexGrid>();
        if (pathfinder == null) pathfinder = FindObjectOfType<Pathfinder>();
        if (factions.Count == 0) {
            factions.Add(new Faction("Player", Color.blue));
            factions.Add(new Faction("AI", Color.red));
        }
        Country.InitializeCountries(); // Inicializa países y banderas generadas por código
    }

    void Start() {
        MostrarBanderasEnUI();
        // Ejemplo: reproducir audio de cultura al iniciar
        foreach (var country in Country.AllCountries) {
            PlayCultureSound(country.culture);
        }

        // Ejemplo: genera una unidad para el jugador y otra para la IA
        if (hexGrid == null) return;
        HexTile playerStart = hexGrid.GetTileAt(0, 0);
        HexTile aiStart = hexGrid.GetTileAt(hexGrid.width - 1, hexGrid.height - 1);

        if (playerStart != null && aiStart != null) {
            var playerUnit = Instantiate(Resources.Load<Unit>("Unit_Infantry"), playerStart.transform.position, Quaternion.identity);
            playerUnit.Init(factions[0], playerStart);
            playerUnit.SetSprite("PlayerUnit"); // Asegúrate de tener el sprite en Resources/Sprites

            var aiUnit = Instantiate(Resources.Load<Unit>("Unit_Infantry"), aiStart.transform.position, Quaternion.identity);
            aiUnit.Init(factions[1], aiStart);
            aiUnit.SetSprite("AIUnit"); // Asegúrate de tener el sprite en Resources/Sprites
        }
    }

    void MostrarBanderasEnUI() {
        if (flagPanel == null || flagImagePrefab == null) return;
        foreach (Transform child in flagPanel) Destroy(child.gameObject); // Limpia panel
        foreach (var country in Country.AllCountries) {
            GameObject flagObj = Instantiate(flagImagePrefab, flagPanel);
            var img = flagObj.GetComponent<Image>();
            if (img != null) img.sprite = country.flag;
            // Muestra la región de mapa como imagen secundaria si el prefab tiene un segundo Image
            var images = flagObj.GetComponentsInChildren<Image>();
            if (images.Length > 1) images[1].sprite = country.mapRegion;
            // Muestra nombre, cultura y características
            var txt = flagObj.GetComponentInChildren<Text>();
            if (txt != null) txt.text = $"{country.name}\n({country.culture})\n{string.Join(", ", country.traits)}";
            // Opcional: cambia el color de fondo según la cultura
            var bg = flagObj.GetComponent<Image>();
            if (bg != null) bg.color = country.mainColor * 0.3f + Color.white * 0.7f;
            // Tooltip: muestra info extra al pasar el mouse (requiere script de tooltip en UI)
            var tooltip = flagObj.GetComponent<CountryTooltip>();
            if (tooltip != null) tooltip.SetInfo(country);
        }
    }

    public void PlayCultureSound(string culture) {
        if (audioSource == null || cultureClips == null) return;
        foreach (var clip in cultureClips) {
            if (clip != null && clip.name.ToLower().Contains(culture.ToLower())) {
                audioSource.PlayOneShot(clip);
                break;
            }
        }
    }

    public void MoveSelectedUnit(HexTile target) {
        if (selectedUnit != null && target != null) {
            selectedUnit.MoveTo(target);
            selectedUnit = null;
            if (pathfinder != null) pathfinder.HidePath();
        }
    }

    public void EndTurn() {
        foreach (Faction faction in factions) {
            faction.RefreshResources();
        }
        AITurn();
    }

    void AITurn() {
        foreach (Unit aiUnit in FindObjectsOfType<Unit>()) {
            if (aiUnit.faction != null && aiUnit.faction.name == "AI") {
                HexTile randomTile = GetRandomMoveTile(aiUnit);
                aiUnit.MoveTo(randomTile);
            }
        }
    }

    HexTile GetRandomMoveTile(Unit unit) {
        if (pathfinder == null || unit == null) return unit?.currentTile;
        List<HexTile> possibleMoves = pathfinder.GetReachableTiles(unit.currentTile, unit.movementRange);
        if (possibleMoves == null || possibleMoves.Count == 0) return unit.currentTile;
        return possibleMoves[Random.Range(0, possibleMoves.Count)];
    }

    // Evento para notificar fin de turno (útil para UI)
    public delegate void TurnEndedHandler(Faction currentFaction);
    public event TurnEndedHandler OnTurnEnded;

    private int currentFactionIndex = 0;

    public void NextTurn() {
        currentFactionIndex = (currentFactionIndex + 1) % factions.Count;
        Faction currentFaction = factions[currentFactionIndex];
        currentFaction.RefreshResources();
        if (currentFaction.name == "AI") {
            AITurn();
        }
        OnTurnEnded?.Invoke(currentFaction);
    }

    public void SelectUnit(Unit unit) {
        if (unit != null && unit.faction == factions[0]) { // Solo selecciona unidades del jugador
            selectedUnit = unit;
        }
    }

    public void DeselectUnit() {
        selectedUnit = null;
        if (pathfinder != null) pathfinder.HidePath();
    }

    // Hook para UI: llamar a NextTurn() desde un botón o tecla
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            NextTurn();
        }
    }
}

public class Unit : MonoBehaviour {
    // Existing variables...

    public void SetSprite(string spriteName) {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = Resources.Load<Sprite>("Sprites/" + spriteName);
    }

    // Existing methods...
}

public class Country {
    public string name;
    public string culture;
    public Sprite flag;
    public Sprite mapRegion; // Nueva propiedad para la región del mapa
    public List<string> traits;
    public Color mainColor; // Color principal para la cultura

    private static List<Country> allCountries = new List<Country>();

    public Country(string name, string culture) {
        this.name = name;
        this.culture = culture;
        this.traits = new List<string>();
        allCountries.Add(this);
        GenerateFlag();
    }

    void GenerateFlag() {
        // Lógica para generar una bandera básica: color sólido con un símbolo simple
        Texture2D flagTexture = new Texture2D(128, 64);
        Color32 color = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), 255);
        for (int y = 0; y < flagTexture.height; y++) {
            for (int x = 0; x < flagTexture.width; x++) {
                flagTexture.SetPixel(x, y, color);
            }
        }
        flagTexture.Apply();

        // Asigna la textura generada a la propiedad flag
        flag = Sprite.Create(flagTexture, new Rect(0, 0, flagTexture.width, flagTexture.height), new Vector2(0.5f, 0.5f));

        // Genera y asigna una región de mapa aleatoria
        mapRegion = Sprite.Create(flagTexture, new Rect(0, 0, flagTexture.width, flagTexture.height), new Vector2(0.5f, 0.5f));
    }

    public static void InitializeCountries() {
        // Aquí puedes inicializar países adicionales o cargar desde un archivo/external source
        new Country("Norway", "Nordic");
        new Country("Spain", "Latin");
        new Country("Japan", "Asian");
        new Country("Egypt", "Arab");
        // ...agrega más países según tu juego
    }

    public static List<Country> AllCountries {
        get { return allCountries; }
    }
}