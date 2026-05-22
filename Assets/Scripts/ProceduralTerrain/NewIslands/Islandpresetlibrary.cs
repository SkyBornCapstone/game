using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  IslandPreset — add more values here as you create new presets
// ─────────────────────────────────────────────────────────────────────────────
public enum IslandPreset
{
    Custom       = 0,   // leaves everything untouched
    Default      = 1,
    Donut        = 2,
    Mesa         = 3,
    Archipelago  = 4,
    Stalactite   = 5,
}

// ─────────────────────────────────────────────────────────────────────────────
//  IslandPresetLibrary — returns density settings for each preset.
//  Gradients are set manually in the Unity Inspector on MarchingCubesIsland.
// ─────────────────────────────────────────────────────────────────────────────
public static class IslandPresetLibrary
{
    /// <summary>
    /// Fills <paramref name="settings"/> with the density values for
    /// <paramref name="preset"/>. Returns false (settings = null) for Custom.
    /// </summary>
    public static bool TryApply(IslandPreset preset, out IslandDensitySettings settings)
    {
        switch (preset)
        {
            case IslandPreset.Default:     settings = Default();     return true;
            case IslandPreset.Donut:       settings = Donut();       return true;
            case IslandPreset.Mesa:        settings = Mesa();        return true;
            case IslandPreset.Archipelago: settings = Archipelago(); return true;
            case IslandPreset.Stalactite:  settings = Stalactite();  return true;
            default: settings = null; return false;
        }
    }

    // ── Density settings ─────────────────────────────────────────────────────

    /// <summary>Balanced, chunky floating island — the original look.</summary>
    static IslandDensitySettings Default() => new IslandDensitySettings
    {
        baseDensity           = 1.2f,
        edgeFalloffStrength   = 2.5f,
        edgeFalloffPower      = 2f,
        topFalloffStrength    = 2f,
        topFalloffPower       = 2f,
        bottomFalloffStrength = 3f,
        bottomFalloffPower    = 3f,
        noiseSeed             = 0,
        noiseScale            = 0.15f,
        noiseVerticalScale    = 0.5f,
        noiseStrength         = 0.6f,
        octaves               = 4,
        persistence           = 0.5f,
        lacunarity            = 2f,
    };


    static IslandDensitySettings Donut() => new IslandDensitySettings
    {
        baseDensity           = 1.2f,
        edgeFalloffStrength   = 1.2f,
        edgeFalloffPower      = 4f,
        topFalloffStrength    = 2.5f,
        topFalloffPower       = 1.5f,
        bottomFalloffStrength = 2.8f,
        bottomFalloffPower    = 2.5f,
        noiseSeed             = 42,
        noiseScale            = 0.18f,
        noiseVerticalScale    = 0.4f,
        noiseStrength         = 0.9f,
        octaves               = 5,
        persistence           = 0.45f,
        lacunarity            = 2.1f,
    };

    /// <summary>Wide flat-topped mesa / plateau with hard cliff edges.</summary>
    static IslandDensitySettings Mesa() => new IslandDensitySettings
    {
        baseDensity           = .4f,
        edgeFalloffStrength   = 3.5f,
        edgeFalloffPower      = 5f,
        topFalloffStrength    = 1.2f,
        topFalloffPower       = 6f,
        bottomFalloffStrength = 1.8f,
        bottomFalloffPower    = 1.5f,
        noiseSeed             = 7,
        noiseScale            = 0.10f,
        noiseVerticalScale    = 0.3f,
        noiseStrength         = 0.35f,
        octaves               = 3,
        persistence           = 0.55f,
        lacunarity            = 1.9f,
    };

    /// <summary>Low thin slab; strong noise fragments it into reef-like pieces.</summary>
    static IslandDensitySettings Archipelago() => new IslandDensitySettings
    {
        baseDensity           = 2.4f,
        edgeFalloffStrength   = 2.0f,
        edgeFalloffPower      = 1.5f,
        topFalloffStrength    = 3.5f,
        topFalloffPower       = 1.2f,
        bottomFalloffStrength = 2.2f,
        bottomFalloffPower    = 1.8f,
        noiseSeed             = 99,
        noiseScale            = 0.28f,
        noiseVerticalScale    = 1.2f,
        noiseStrength         = 1.1f,
        octaves               = 6,
        persistence           = 0.6f,
        lacunarity            = 2.3f,
    };

    /// <summary>Thin body with massive jagged stalactites hanging below.</summary>
    static IslandDensitySettings Stalactite() => new IslandDensitySettings
    {
        baseDensity           = 1.0f,
        edgeFalloffStrength   = 2.2f,
        edgeFalloffPower      = 2.5f,
        topFalloffStrength    = 2.8f,
        topFalloffPower       = 1.8f,
        bottomFalloffStrength = 1.5f,
        bottomFalloffPower    = 8f,
        noiseSeed             = 13,
        noiseScale            = 0.20f,
        noiseVerticalScale    = 1.8f,
        noiseStrength         = 0.85f,
        octaves               = 5,
        persistence           = 0.5f,
        lacunarity            = 2.2f,
    };
}