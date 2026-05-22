using UnityEngine;

[System.Serializable]
public class IslandDensitySettings
{
    [Header("Core Shape")]
    [Tooltip("How strongly the interior is filled. Higher = thicker island.")]
    public float baseDensity = 1.2f;

    [Header("Edge Falloff — horizontal silhouette")]
    public float edgeFalloffStrength = 2.5f;
    [Tooltip("Higher power = sharper edge. Lower = gentler slope.")]
    public float edgeFalloffPower = 2f;

    [Header("Top Falloff — hill shape")]
    public float topFalloffStrength = 2f;
    [Tooltip("Higher power = flatter top with sudden dropoff at crown.")]
    public float topFalloffPower = 2f;

    [Header("Bottom Falloff — torn underside")]
    public float bottomFalloffStrength = 3f;
    [Tooltip("Higher power = more jagged, stalactite-like underside.")]
    public float bottomFalloffPower = 3f;

    [Header("Noise")]
    public int   noiseSeed            = 0;
    public float noiseScale           = 0.15f;
    [Tooltip("Squish or stretch noise vertically.")]
    public float noiseVerticalScale   = 0.5f;
    public float noiseStrength        = 0.6f;
    public int   octaves              = 4;
    [Range(0f, 1f)]
    public float persistence          = 0.5f;
    public float lacunarity           = 2f;
}