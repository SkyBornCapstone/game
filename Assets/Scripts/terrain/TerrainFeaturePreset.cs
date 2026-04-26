using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "TerrainFeaturePreset", menuName = "Island Decoration/Terrain Feature Preset")]
    public class TerrainFeaturePreset : ScriptableObject
    {
        [Header("Prefab")] [Tooltip("The terrain feature prefab to spawn (tree, rock, etc.)")]
        public GameObject featurePrefab;

        [Header("Spawn Settings")] [Tooltip("Probability of spawning this feature (0-1)")] [Range(0f, 1f)]
        public float spawnProbability = 1f;

        [Header("Placement")] [Tooltip("Height offset from surface")]
        public float heightOffset = 0f;

        [Tooltip("Align to surface normal")] public bool alignToSurface = true;

        [Tooltip("Max slope angle to allow spawning (degrees)")] [Range(0f, 90f)]
        public float maxSlopeAngle = 45f;

        [Header("Rotation")] [Tooltip("Random rotation on Y axis")]
        public bool randomYRotation = true;

        [Tooltip("Random rotation on all axes")]
        public bool randomFullRotation = false;

        [Header("Scale")] [Tooltip("Randomize scale")]
        public bool randomizeScale = true;

        [Tooltip("Min/Max scale multiplier")] public Vector2 scaleRange = new(0.8f, 1.2f);

        [Header("Clustering")] [Tooltip("Allow spawning in clusters")]
        public bool allowClustering = false;

        [Tooltip("If clustering, min/max cluster size")]
        public Vector2Int clusterSizeRange = new(2, 5);

        [Tooltip("Cluster spread radius")] public float clusterRadius = 3f;
    }
}