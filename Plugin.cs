using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.valheim.valeriusnoobshield";
    public const string PluginName = "Valerius Noob Shield";
    public const string PluginVersion = "1.0.3";

    // A creature may spawn up to this many biome tiers
    // below its native biome.
    public const int MaxBiomeOverflow = 2;

    internal static ManualLogSource Log;

    private void Awake()
    {
        Log = Logger;

        new Harmony(PluginGuid).PatchAll();

        Log.LogInfo(
            $"Loaded. Max downward biome overflow: " +
            $"{MaxBiomeOverflow}"
        );
    }
}

public static class BiomeUtils
{
    private static readonly (Heightmap.Biome biome, int tier)[] Biomes =
    {
        (Heightmap.Biome.Meadows, 0),
        (Heightmap.Biome.BlackForest, 1),
        (Heightmap.Biome.Swamp, 2),
        (Heightmap.Biome.Mountain, 3),
        (Heightmap.Biome.Plains, 4),
        (Heightmap.Biome.Mistlands, 5),
        (Heightmap.Biome.AshLands, 6),
        (Heightmap.Biome.DeepNorth, 7)
    };

    public static int GetTier(Heightmap.Biome biome)
    {
        foreach (var entry in Biomes)
        {
            if ((biome & entry.biome) != 0)
                return entry.tier;
        }

        return -1;
    }

    public static string GetName(int tier)
    {
        switch (tier)
        {
            case 0: return "Meadows";
            case 1: return "Black Forest";
            case 2: return "Swamp";
            case 3: return "Mountain";
            case 4: return "Plains";
            case 5: return "Mistlands";
            case 6: return "Ashlands";
            case 7: return "Deep North";
            default: return "Unknown";
        }
    }

    public static string FormatDifference(int difference)
    {
        if (difference == 0)
            return "native";

        int distance = Mathf.Abs(difference);
        string direction = difference > 0 ? "down" : "up";

        return distance == 1
            ? $"1 tier {direction}"
            : $"{distance} tiers {direction}";
    }
}

[HarmonyPatch(typeof(SpawnSystem), "UpdateSpawning")]
public static class SpawnSystemNativeBiomePatch
{
    private static readonly Dictionary<string, int> NativeBiomeTiers =
        new Dictionary<string, int>();

    private static bool registryBuilt;

    private static void Prefix(SpawnSystem __instance)
    {
        if (registryBuilt ||
            __instance.m_spawnLists == null ||
            __instance.m_spawnLists.Count == 0)
        {
            return;
        }

        foreach (SpawnSystemList spawnList in __instance.m_spawnLists)
        {
            if (spawnList?.m_spawners == null)
                continue;

            foreach (SpawnSystem.SpawnData spawn in spawnList.m_spawners)
            {
                if (spawn?.m_prefab == null ||
                    !string.IsNullOrEmpty(spawn.m_requiredGlobalKey))
                {
                    continue;
                }

                int nativeTier = BiomeUtils.GetTier(spawn.m_biome);

                if (nativeTier < 0)
                    continue;

                string prefabName = spawn.m_prefab.name;

                if (!NativeBiomeTiers.TryGetValue(
                        prefabName,
                        out int existingTier) ||
                    nativeTier < existingTier)
                {
                    NativeBiomeTiers[prefabName] = nativeTier;
                }
            }
        }

        if (NativeBiomeTiers.Count == 0)
            return;

        registryBuilt = true;

        Plugin.Log.LogInfo(
            $"Native biome registry built: " +
            $"{NativeBiomeTiers.Count} creatures"
        );
    }

    public static bool TryGetNativeTier(
        string prefabName,
        out int nativeTier)
    {
        return NativeBiomeTiers.TryGetValue(
            prefabName,
            out nativeTier
        );
    }
}

[HarmonyPatch(typeof(SpawnSystem), "IsSpawnPointGood")]
public static class SpawnSystemBiomeProgressionPatch
{
    private static void Postfix(
        SpawnSystem.SpawnData spawn,
        ref Vector3 spawnPoint,
        ref bool __result)
    {
        if (!__result ||
            spawn?.m_prefab == null ||
            spawn.m_prefab.name == "projectile_FimbulvinterMeteor" ||
            string.IsNullOrEmpty(spawn.m_requiredGlobalKey))
        {
            return;
        }

        if (!SpawnSystemNativeBiomePatch.TryGetNativeTier(
                spawn.m_prefab.name,
                out int nativeTier))
        {
            return;
        }

        if (ZoneSystem.instance == null)
            return;

        ZoneSystem.instance.GetGroundData(
            ref spawnPoint,
            out _,
            out Heightmap.Biome biome,
            out _,
            out _
        );

        int currentTier = BiomeUtils.GetTier(biome);

        if (currentTier < 0)
            return;

        int tierDifference = nativeTier - currentTier;

        // Only restrict progression spawns moving more than
        // two tiers downward. Upward spawning is unrestricted.
        if (tierDifference <= Plugin.MaxBiomeOverflow)
            return;

        __result = false;

        Plugin.Log.LogInfo(
            $"BLOCKED " +
            $"{spawn.m_prefab.name} | " +
            $"{BiomeUtils.GetName(nativeTier)} -> " +
            $"{BiomeUtils.GetName(currentTier)} | " +
            $"{BiomeUtils.FormatDifference(tierDifference)}"
        );
    }
}

[HarmonyPatch(typeof(SpawnSystem), "Spawn")]
public static class SpawnSystemAllowedLogPatch
{
    private static void Postfix(
        SpawnSystem.SpawnData critter,
        Vector3 spawnPoint)
    {
        if (critter?.m_prefab == null ||
            critter.m_prefab.name == "projectile_FimbulvinterMeteor")
        {
            return;
        }

        if (!SpawnSystemNativeBiomePatch.TryGetNativeTier(
                critter.m_prefab.name,
                out int nativeTier))
        {
            return;
        }

        if (ZoneSystem.instance == null)
            return;

        ZoneSystem.instance.GetGroundData(
            ref spawnPoint,
            out _,
            out Heightmap.Biome biome,
            out _,
            out _
        );

        int currentTier = BiomeUtils.GetTier(biome);

        if (currentTier < 0)
            return;

        int tierDifference = nativeTier - currentTier;

        Plugin.Log.LogInfo(
            $"ALLOWED " +
            $"{critter.m_prefab.name} | " +
            $"{BiomeUtils.GetName(nativeTier)} -> " +
            $"{BiomeUtils.GetName(currentTier)} | " +
            $"{BiomeUtils.FormatDifference(tierDifference)}"
        );
    }
}

[HarmonyPatch(typeof(RandEventSystem))]
public static class RandEventSystemProgressionPatch
{
    [HarmonyPatch("GetValidEventPoints")]
    [HarmonyPostfix]
    private static void Postfix(
        RandomEvent ev,
        ref List<Vector3> __result)
    {
        if (__result == null ||
            __result.Count == 0 ||
            ev?.m_spawn == null ||
            ev.m_spawn.Count == 0 ||
            ZoneSystem.instance == null)
        {
            return;
        }

        __result.RemoveAll(position =>
        {
            ZoneSystem.instance.GetGroundData(
                ref position,
                out _,
                out Heightmap.Biome biome,
                out _,
                out _
            );

            int currentTier = BiomeUtils.GetTier(biome);

            if (currentTier < 0)
                return false;

            foreach (SpawnSystem.SpawnData spawn in ev.m_spawn)
            {
                if (spawn?.m_prefab == null ||
                    string.IsNullOrEmpty(spawn.m_requiredGlobalKey))
                {
                    continue;
                }

                if (!SpawnSystemNativeBiomePatch.TryGetNativeTier(
                        spawn.m_prefab.name,
                        out int nativeTier))
                {
                    continue;
                }

                int tierDifference = nativeTier - currentTier;

                if (tierDifference > Plugin.MaxBiomeOverflow)
                {
                    Plugin.Log.LogInfo(
                        $"BLOCKED RAID " +
                        $"{ev.m_name} | " +
                        $"{spawn.m_prefab.name} | " +
                        $"{BiomeUtils.GetName(nativeTier)} -> " +
                        $"{BiomeUtils.GetName(currentTier)} | " +
                        $"{BiomeUtils.FormatDifference(tierDifference)}"
                    );

                    return true;
                }
            }

            return false;
        });
    }
}