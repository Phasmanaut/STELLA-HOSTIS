using UnityEngine;

public static class GooSpawner
{
    // All percentages below are calibrated for a "basic" enemy worth basePoints;
    // tougher enemies (higher pointValue) get a better chance to drop, and a better chance of dropping more.
    public static float basePoints = 50f; // Enemy_A's point value
    public static float baseDropChance = 0.3f; // 30% chance to drop anything, at basePoints
    public static float baseChance2 = 0.25f; // chance of dropping exactly 2, given it drops, at basePoints
    public static float baseChance3 = 0.05f; // chance of dropping exactly 3, given it drops, at basePoints
    // remaining chance (1 - baseChance2 - baseChance3 = 70% at basePoints) drops exactly 1

    public static void SpawnGoo(GameObject gooPrefab, Vector3 position, int pointValue)
    {
        float richness = pointValue / basePoints;

        float dropChance = Mathf.Clamp01(baseDropChance * richness);
        if (Random.value > dropChance) return; // no drop this time

        float richnessBonus = Mathf.Max(0f, richness - 1f); // 0 for basic enemies, grows for higher-point ones
        float chance3 = baseChance3 + richnessBonus * 0.10f;
        float chance2 = baseChance2 + richnessBonus * 0.20f;

        float roll = Random.value;
        int count = roll < chance3 ? 3 : roll < chance3 + chance2 ? 2 : 1;

        for (int i = 0; i < count; i++)
        {
            Object.Instantiate(gooPrefab, position, Quaternion.identity);
        }
    }
}
