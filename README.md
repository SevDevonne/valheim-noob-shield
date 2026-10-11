# Valerius Noob Shield

> **Keep the late-game mobs out of your early-game base.**

Prevents progression-gated creatures and raids from spawning more than **2 biomes below their native biome**.

Useful for multiplayer servers where some players have progressed further than others, while still keeping early-game areas safe for newer players.

> *Also, I'm really tired of Seekers and Charred Warriors attacking me while I'm building my sick ahh base in the Meadows.*

---

## How It Works

Each biome has a progression tier:

| Biome        | Tier |
| :----------- | ---: |
| Meadows      |  `0` |
| Black Forest |  `1` |
| Swamp        |  `2` |
| Mountain     |  `3` |
| Plains       |  `4` |
| Mistlands    |  `5` |
| Ashlands     |  `6` |
| Deep North   |  `7` |

Progression-gated creatures can spawn up to **2 biome tiers below** their native biome.

**Raid events are also treated as progression-gated events.**

Spawning into later biomes is unrestricted.

Normal, non-progression-gated spawns are unchanged.

### Example

An Ashlands creature has a native biome tier of **6**:

| Current Biome |   Result  |
| :------------ | :-------: |
| Meadows       | ❌ Blocked |
| Black Forest  | ❌ Blocked |
| Swamp         | ❌ Blocked |
| Mountain      | ❌ Blocked |
| Plains        | ✅ Allowed |
| Mistlands     | ✅ Allowed |
| Ashlands      | ✅ Allowed |
| Deep North    | ✅ Allowed |

---

## Console Logging

Allowed and blocked spawns are logged to the console for debugging.

**Allowed :**

```text
[Info   :Valerius Noob Shield] ALLOWED Greydwarf | Black Forest -> Meadows | 1 tier down
```

**Blocked:**

```text
[Info   :Valerius Noob Shield] BLOCKED Seeker | Mistlands -> Meadows | 5 tiers down
```

The log includes:

* Creature
* Native biome
* Current biome
* Tier difference

Logs for spawns with the prefab name "projectile_FimbulvinterMeteor" are suppressed due to sheer amount.

**Blocked Raid:**
```text
[Info   :Valerius Noob Shield] BLOCKED RAID army_seekers | Seeker | Mistlands -> Meadows | 5 tiers down
```

---
## Git 

The project is available on Github at `https://github.com/SevDevonne/valheim-noob-shield`

---

## Artificial Intelligence Usage

Artificial intelligence was used in the creation of this mod. The code has been proofread and the mod has been tested thoroughly in-game.

---

## Testing

* Game-generated spawn rates across various world-progression and biome combinations
* Command-generated raids in various biomes
* Compatibility testing with other mods

No incompatibilities found so far.

> **Found an incompatibility?** Let me know and I'll add it here.

---

## Current Issues & Shortcomings

* Not tested with mods that add creatures or modify existing creature properties.
