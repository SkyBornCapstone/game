using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "EnemySpawnPreset", menuName = "Island Decoration/Enemy Spawn Preset")]
    public class EnemySpawnPreset : ScriptableObject
    {
        [Header("Prefab")] [Tooltip("The enemy prefab to spawn")]
        public GameObject enemyPrefab;

        [Header("Spawn Settings")] [Tooltip("Probability of spawning this enemy (0-1)")] [Range(0f, 1f)]
        public float spawnProbability = 1f;

        [Header("Placement")] [Tooltip("Height offset from surface")]
        public float heightOffset = 0f;

        [Tooltip("Max slope angle to allow spawning (degrees)")] [Range(0f, 90f)]
        public float maxSlopeAngle = 30f;

        [Tooltip("Minimum distance from island edge")]
        public float edgeBuffer = 2f;

        [Header("Rotation")] [Tooltip("Random rotation on Y axis")]
        public bool randomYRotation = true;

        [Header("AI Configuration")] [Tooltip("Requires NavMesh surface")]
        public bool requiresNavMesh = true;

        [Tooltip("Patrol area radius around spawn point")]
        public float patrolRadius = 10f;

        [Header("Group Spawning")] [Tooltip("Spawn in groups")]
        public bool spawnInGroups = false;

        [Tooltip("If grouping, min/max group size")]
        public Vector2Int groupSizeRange = new(2, 4);

        [Tooltip("Group spread radius")] public float groupRadius = 5f;
    }
}