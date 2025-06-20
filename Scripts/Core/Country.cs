using UnityEngine;
using System.Collections.Generic;

public enum GovernmentType { Democracy, Monarchy, Republic, Dictatorship }
public enum EconomyType { Capitalist, Socialist, Mixed, Tribal }

public class Country {
    public string Name { get; private set; }
    public string Culture { get; private set; }
    public Sprite Flag { get; private set; }
    public Sprite MapRegion { get; private set; }
    public Color MainColor { get; private set; }
    public string[] Traits { get; private set; }
    public GovernmentType Government { get; private set; }
    public EconomyType Economy { get; private set; }
    public int Population { get; set; }
    public float GDP { get; set; }
    public Dictionary<Country, int> Relations = new Dictionary<Country, int>();
    public List<string> EventLog = new List<string>();

    public Country(string name, string culture, GovernmentType gov, EconomyType eco, int population, float gdp) {
        Name = name;
        Culture = culture;
        Government = gov;
        Economy = eco;
        Population = population;
        GDP = gdp;
        Flag = GenerateFlagSprite(name, culture);
        MapRegion = GenerateMapRegionSprite(name);
        MainColor = GetColorByCulture(culture);
        Traits = GetTraitsByCulture(culture);
    }

    static Sprite GenerateFlagSprite(string name, string culture) {
        Texture2D tex = new Texture2D(64, 32);
        Color baseColor = GetColorByCulture(culture);
        Color accent = GetAccentColorByCountry(name);
        // Patrón: franja superior base, franja inferior accent
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, y < tex.height / 2 ? baseColor : accent);
        // Detalle: círculo central
        DrawCircle(tex, tex.width / 2, tex.height / 2, tex.height / 5, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    static Sprite GenerateMapRegionSprite(string name) {
        Texture2D tex = new Texture2D(32, 32);
        Color color = GetAccentColorByCountry(name) * 0.7f;
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, color);
        // Detalle: línea diagonal
        for (int i = 0; i < tex.width; i++)
            tex.SetPixel(i, i * tex.height / tex.width, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    static Color GetAccentColorByCountry(string name) {
        switch (name) {
            case "Norway": return new Color(0.1f, 0.2f, 0.7f);
            case "Spain": return new Color(1f, 0.85f, 0.2f);
            case "Japan": return new Color(0.9f, 0.1f, 0.1f);
            case "Egypt": return new Color(0.9f, 0.8f, 0.5f);
            default: return Color.gray;
        }
    }

    static void DrawCircle(Texture2D tex, int cx, int cy, int r, Color col) {
        for (int x = -r; x <= r; x++)
            for (int y = -r; y <= r; y++)
                if (x * x + y * y <= r * r)
                    tex.SetPixel(cx + x, cy + y, col);
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

    public void AddEvent(string evt) {
        EventLog.Add(evt);
    }

    public void SetRelation(Country other, int value) {
        Relations[other] = Mathf.Clamp(value, -100, 100);
    }

    public int GetRelation(Country other) {
        return Relations.ContainsKey(other) ? Relations[other] : 0;
    }

    public string GetDescription() {
        return $"{Name} ({Culture})\nPoblación: {Population}\nPIB: {GDP}M\nGobierno: {Government}\nEconomía: {Economy}\nCaracterísticas: {string.Join(", ", Traits)}";
    }
}
