public static class CombatSystem {
    public static void ResolveCombat(Unit attacker, Unit defender) {
        float terrainBonus = defender.currentTile.defenseBonus * 0.1f;
        float typeAdvantage = GetTypeAdvantage(attacker.type, defender.type);
        int damage = Mathf.RoundToInt(attacker.attack * typeAdvantage - defender.defense * (1 + terrainBonus));

        defender.health -= Mathf.Max(1, damage);
        if (defender.health <= 0) {
            Object.Destroy(defender.gameObject);
            attacker.MoveTo(defender.currentTile);
        }
    }

    static float GetTypeAdvantage(UnitType attacker, UnitType defender) {
        if (attacker == UnitType.Cavalry && defender == UnitType.Infantry) return 1.5f;
        if (attacker == UnitType.Infantry && defender == UnitType.Archer) return 1.5f;
        if (attacker == UnitType.Archer && defender == UnitType.Cavalry) return 1.5f;
        return 1.0f;
    }
}
