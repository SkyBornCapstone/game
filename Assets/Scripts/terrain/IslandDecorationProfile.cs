using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "IslandDecorationProfile", menuName = "Island Decoration/Decoration Profile")]
    public class IslandDecorationProfile : ScriptableObject
    {
        [Serializable]
        public class ItemSpawnEntry
        {
            [Tooltip("The item prefab to spawn")] public GameObject itemPrefab;

            [Tooltip("Probability of spawning this item (0-1)")] [Range(0f, 1f)]
            public float spawnProbability = 0.5f;
        }

        [Serializable]
        public class TerrainFeatureSpawnEntry
        {
            public TerrainFeaturePreset preset;

            [Tooltip("Min/Max number of features to spawn")]
            public Vector2Int spawnCountRange = new(5, 15);
        }

        [Serializable]
        public class EnemySpawnEntry
        {
            public EnemySpawnPreset preset;

            [Tooltip("Min/Max number of enemies to spawn")]
            public Vector2Int spawnCountRange = new(1, 3);
        }

        [Header("Profile Info")] [Tooltip("Profile name (e.g., 'Normal Island', 'Rocky Island', 'Treasure Island')")]
        public string profileName = "Island Profile";

        [Header("Items")] [Tooltip("Total number of item spawn attempts")]
        public int itemSpawnAttempts = 10;

        [Tooltip("List of items that can spawn with their probabilities")]
        public List<ItemSpawnEntry> items = new();

        [Header("Terrain Features")] [Tooltip("List of terrain features to spawn")]
        public List<TerrainFeatureSpawnEntry> terrainFeatures = new();

        [Header("Enemies")] [Tooltip("List of enemies to spawn")]
        public List<EnemySpawnEntry> enemies = new();

        [Header("General Settings")] [Tooltip("Overall spawn density multiplier")] [Range(0f, 2f)]
        public float densityMultiplier = 1f;

        [Tooltip("Minimum distance between spawned objects")]
        public float minSpacing = 1f;

        [Tooltip("Maximum spawn attempts before giving up")]
        public int maxSpawnAttempts = 100;
    }
}