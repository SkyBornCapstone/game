using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    [Serializable]
    public struct IslandBucket
    {
        [Serializable]
        public struct DecorationProfileEntry
        {
            [Tooltip("The decoration profile (e.g., 'Normal Island', 'Rocky Island', 'Treasure Island')")]
            public IslandDecorationProfile profile;

            [Tooltip("Weight for selecting this profile")]
            public int weight;
        }

        public string name;

        [Tooltip("Weight of this bucket compared to individual islands and other buckets.")]
        public int weight;

        [Tooltip(
            "Optional exclusion radius for all islands in this bucket. If greater than 0, this overrides the individual island's exclusion radius.")]
        public float exclusionRadius;

        [Tooltip(
            "The island types in this bucket. Their individual weights provide the relative probability of being chosen from within this bucket.")]
        public List<IslandData> islands;

        [Tooltip("List of decoration profiles to randomly choose from for each island. Weighted selection.")]
        public List<DecorationProfileEntry> decorationProfiles;
    }

    [Serializable]
    public struct IslandData
    {
        public GameObject prefab;

        public TerrainNoiseData terrainData;

        [Tooltip("Likelihood of this island being chosen.")]
        public int weight;

        [Tooltip("Minimum distance from this island center that another island can spawn.")]
        public float exclusionRadius;

        [Tooltip(
            "Optional decoration profile for spawning items, terrain features, and enemies. Leave empty to use bucket defaults.")]
        public IslandDecorationProfile decorationProfile;
    }

    [Serializable]
    public struct TerrainNoiseData
    {
        [Header("Noise Settings")] public int seed;
        public int octaves;
        public float persistence;
        public float lacunarity;
        public float noiseScale;

        [Header("Offsets")] public Vector2 offset;

        [Header("Visuals")] public Gradient colorGradient;
        public Material material;

        [Header("Mesh Settings")] public float heightMeshMultiplier;
        public AnimationCurve heightCurve;
    }
}