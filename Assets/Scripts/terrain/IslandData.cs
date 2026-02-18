using UnityEngine;

namespace Terrain
{
    [System.Serializable]
    public struct IslandData
    {
        public GameObject prefab;

        public TerrainNoiseData terrainData;

        [Tooltip("Likelihood of this island being chosen.")]
        public int weight;
    }

    [System.Serializable]
    public struct TerrainNoiseData
    {
        [Header("Noise Settings")]
        public int seed;
        public int octaves;
        public float persistence;
        public float lacunarity;
        public float noiseScale;

        [Header("Offsets")]
        public Vector2 offset;

        [Header("Visuals")]
        public Gradient colorGradient;
        public Material material;

        [Header("Mesh Settings")]
        public float heightMeshMultiplier;
        public AnimationCurve heightCurve;
    }
}