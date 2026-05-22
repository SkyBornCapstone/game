using UnityEngine;

public class DensityField
{
    public readonly int ResX, ResY, ResZ;
    public readonly float[,,] Values;

    public DensityField(int resX, int resY, int resZ)
    {
        ResX = resX;
        ResY = resY;
        ResZ = resZ;
        Values = new float[resX, resY, resZ];
    }

    // ─────────────────────────────────────────────
    //  Public entry points
    // ─────────────────────────────────────────────

    public void PopulateSphere()
    {
        Vector3 center = new Vector3(ResX, ResY, ResZ) * 0.5f;
        float radius = Mathf.Min(ResX, ResY, ResZ) * 0.35f;

        for (int x = 0; x < ResX; x++)
        for (int y = 0; y < ResY; y++)
        for (int z = 0; z < ResZ; z++)
            Values[x, y, z] = Vector3.Distance(new Vector3(x, y, z), center) - radius;
    }

    public void PopulateFlatSlab()
    {
        float midY = ResY * 0.5f;

        for (int x = 0; x < ResX; x++)
        for (int y = 0; y < ResY; y++)
        for (int z = 0; z < ResZ; z++)
            Values[x, y, z] = y - midY;
    }

    /// <summary>
    /// The main island shape. Combines:
    ///   - A solid core
    ///   - Edge falloff to round the silhouette horizontally
    ///   - Top falloff  (smooth hills)
    ///   - Bottom falloff (aggressive torn underside)
    ///   - 3D noise on top of all of that
    /// </summary>
    public void PopulateIsland(IslandDensitySettings s)
    {
        for (int x = 0; x < ResX; x++)
        for (int y = 0; y < ResY; y++)
        for (int z = 0; z < ResZ; z++)
        {
            // Normalized 0-1 coordinates
            float nx = x / (float)(ResX - 1);   // 0 = left  edge,  1 = right edge
            float ny = y / (float)(ResY - 1);   // 0 = bottom,      1 = top
            float nz = z / (float)(ResZ - 1);

            // 1. Start solid (negative = inside the surface)
            float density = -s.baseDensity;

            // 2. Radial edge falloff — carves away toward horizontal edges
            float dx = nx - 0.5f;
            float dz = nz - 0.5f;
            float radialDist = Mathf.Sqrt(dx * dx + dz * dz) * 2f; // 0 center → 1 at corner
            density += Mathf.Pow(radialDist, s.edgeFalloffPower) * s.edgeFalloffStrength;

            // 3. Top falloff — curves the terrain surface upward
            //    ny close to 1 = top of grid, we push density positive there
            float topFade = Mathf.Pow(ny, s.topFalloffPower);
            density += topFade * s.topFalloffStrength;

            // 4. Bottom falloff — aggressive, makes it look torn
            //    ny close to 0 = bottom of grid
            float bottomFade = Mathf.Pow(1f - ny, s.bottomFalloffPower);
            density += bottomFade * s.bottomFalloffStrength;

            // 5. 3D Perlin noise — surface detail and variation
            float noiseVal = SampleNoise(
                x * s.noiseScale,
                y * s.noiseScale * s.noiseVerticalScale,
                z * s.noiseScale,
                s.octaves,
                s.persistence,
                s.lacunarity,
                s.noiseSeed
            );
            density += noiseVal * s.noiseStrength;

            Values[x, y, z] = density;
        }
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────
    public void PadBorders(float positiveValue = 1f)
    {
        for (int x = 0; x < ResX; x++)
        for (int y = 0; y < ResY; y++)
        for (int z = 0; z < ResZ; z++)
        {
            bool isBorder = x == 0 || x == ResX - 1 ||
                            y == 0 || y == ResY - 1 ||
                            z == 0 || z == ResZ - 1;

            if (isBorder)
                Values[x, y, z] = positiveValue;
        }
    }
    float SampleNoise(float x, float y, float z, int octaves, float persistence, float lacunarity, int seed)
    {
        float value     = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxValue  = 0f;

        // Offset each octave by the seed so different seeds give different shapes
        float seedOffset = seed * 127.3f;

        for (int i = 0; i < octaves; i++)
        {
            value    += Mathf.PerlinNoise(
                            x * frequency + seedOffset,
                            z * frequency + seedOffset        // XZ slice
                        )
                        * amplitude;

            // Add a second sample offset in Y to get true 3D variation
            value    += Mathf.PerlinNoise(
                            x * frequency + seedOffset + 31.4f,
                            y * frequency + seedOffset + 31.4f
                        )
                        * amplitude * 0.5f;

            maxValue  += amplitude * 1.5f;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // Remap 0-1 → -1 to 1 so noise can push density in both directions
        float normalised = (maxValue > 0f) ? (value / maxValue) : 0f;
        return normalised * 2f - 1f;
    }
}