using UnityEngine;
using System.Collections.Generic;

public class Country {
    public string name;
    public string culture;
    public Sprite flag;
    public Sprite mapRegion;
    public Color mainColor;
    public string[] traits;

    public static List<Country> AllCountries = new List<Country>();

    public Country(string name, string culture) {
        this.name = name;
        this.culture = culture;
        this.flag = GenerateFlagSprite(name, culture);
        this.mapRegion = GenerateMapRegionSprite(name);
        this.mainColor = GetColorByCulture(culture);
        this.traits = GetTraitsByCulture(culture);
    }

    // Genera un sprite de bandera por código (placeholder: color sólido)
    static Sprite GenerateFlagSprite(string name, string culture) {
        Texture2D tex = new Texture2D(64, 32);
        Color color = GetColorByCulture(culture);
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    // Genera un sprite de región de mapa por código (placeholder: color sólido más oscuro)
    static Sprite GenerateMapRegionSprite(string name) {
        Texture2D tex = new Texture2D(32, 32);
        Color color = Color.gray * 0.7f;
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    static Color GetColorByCulture(string culture) {
        switch (culture) {
            case "Nordic": return Color.cyan;
            case "Latin": return Color.red;
            case "Asian": return Color.yellow;
            case "Arab": return new Color(0.8f, 0.7f, 0.2f);
            default: return Color.white;
        }
    }

    static string[] GetTraitsByCulture(string culture) {
        switch (culture) {
            case "Nordic": return new[] { "Resilient", "Explorer" };
            case "Latin": return new[] { "Passionate", "Strategist" };
            case "Asian": return new[] { "Disciplined", "Innovative" };
            case "Arab": return new[] { "Trader", "Adaptable" };
            default: return new[] { "Neutral" };
        }
    }

    // Inicializa todos los países por código
    public static void InitializeCountries() {
        AllCountries.Clear();
        AllCountries.Add(new Country("Norway", "Nordic"));
        AllCountries.Add(new Country("Spain", "Latin"));
        AllCountries.Add(new Country("Japan", "Asian"));
        AllCountries.Add(new Country("Egypt", "Arab"));
        // Agrega más países aquí si lo deseas
    }
}
