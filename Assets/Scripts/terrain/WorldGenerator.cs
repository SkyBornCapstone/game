using System.Collections.Generic;
using ProceduralTerrain;
using PurrNet;
using Unity.AI.Navigation;
using UnityEngine;

namespace Terrain
{
    public class WorldGenerator : NetworkBehaviour
    {
        [Header("Network Synchronization")]
        [Tooltip("World seed for deterministic generation. Set by server, synced to clients.")]
        public SyncVar<int> worldSeed = new(0);

        [Header("World Shape Settings")] [Tooltip("Radius of the inner circular world.")]
        public float innerRadius = 500f;

        [Tooltip("Width of the empty ring outside the inner world.")]
        public float emptyRingWidth = 200f;

        [Tooltip("Width of the outermost ring where specific islands spawn.")]
        public float outerRingWidth = 200f;

        [Tooltip("Probability of attempting to spawn an island at any given valid point (0.0 to 1.0).")] [Range(0f, 1f)]
        public float innerSpawnProbability = 0.01f;

        [Range(0f, 1f)] public float outerSpawnProbability = 0.10f;

        [Header("Vertical Variation")] public float minHeight = -25f;
        public float maxHeight = 25f;

        [Header("Inner World Island Configuration")] [Tooltip("The specific island to always spawn at (0,0,0).")]
        public IslandData startingIsland;

        [Tooltip("List of individual island types to generate.")]
        public List<IslandData> individualIslands;

        [Tooltip(
            "List of island buckets. A bucket is chosen first by its weight, then an island within it is chosen by its local weight.")]
        public List<IslandBucket> islandBuckets;

        [Header("Outer Ring Island Configuration")]
        [Tooltip("The bucket of islands to use exclusively for the outer ring.")]
        public IslandBucket outerRingBucket;

        public NavMeshSurface navMesh;

        // Internal list to track placed islands for exclusion checking
        private struct PlacedIsland
        {
            public Vector3 position;
            public float radius;
            public string name; // Debugging
        }

        private List<PlacedIsland> placedIslands = new List<PlacedIsland>();

        private bool startingIslandSet = false;

        private FarlandsVisibilityManager _visibilityManager;

        // Trigger worldgen on start
        protected override void OnSpawned()
        {
            TryGetComponent(out _visibilityManager);
            // Set visibility threshold to 150m before the inner radius
            if (_visibilityManager != null)
            {
                _visibilityManager.visibilityThresholdRadius = Mathf.Max(0, innerRadius - 150f);
            }

            if (isServer)
            {
                worldSeed.value = Random.Range(int.MinValue, int.MaxValue);
            }

            GenerateWorld();
            if (navMesh) navMesh.BuildNavMesh();
        }

        [ContextMenu("Generate World")]
        public void GenerateWorld()
        {
            ClearWorld();

            Random.InitState(worldSeed.value);
            placedIslands.Clear();
            List<Vector2Int> innerPoints = new List<Vector2Int>();
            List<Vector2Int> outerPoints = new List<Vector2Int>();

            float maxRadius = innerRadius + emptyRingWidth + outerRingWidth;
            int maxRadiusInt = Mathf.CeilToInt(maxRadius);

            for (int x = -maxRadiusInt; x <= maxRadiusInt; x++)
            {
                for (int z = -maxRadiusInt; z <= maxRadiusInt; z++)
                {
                    float dist = Mathf.Sqrt(x * x + z * z);
                    if (dist <= innerRadius)
                    {
                        innerPoints.Add(new Vector2Int(x, z));
                    }
                    else if (dist > innerRadius + emptyRingWidth && dist <= maxRadius)
                    {
                        outerPoints.Add(new Vector2Int(x, z));
                    }
                }
            }

            // Place Starting Island at (0,0)
            SpawnIsland(startingIsland, Vector3.zero);

            // Process Inner Points
            // Instead of looping through all points with a probability of spawning at each one,
            // we will shuffle the points and then trim the list to the number of attempts we want
            // Ex: instead of checking 100 points with a 10% spawn probability, we will check 10 random
            // points with a 100% spawn probability
            Shuffle(innerPoints);
            int innerTotalWeight = GetTotalWeight();
            int innerAttempts = Mathf.RoundToInt(innerPoints.Count * innerSpawnProbability);
            for (int i = 0; i < innerAttempts; i++)
            {
                Vector2Int point = innerPoints[i];
                Vector3 candidatePos = new Vector3(point.x, 0, point.y);
                IslandData selectedIsland = SelectWeightedIsland(innerTotalWeight);

                if (IsPositionObstructed(candidatePos, selectedIsland.exclusionRadius))
                    continue;

                float height = Random.Range(minHeight, maxHeight);
                Vector3 finalPos = candidatePos + Vector3.up * height;
                SpawnIsland(selectedIsland, finalPos);
            }

            // Process Outer Points
            Shuffle(outerPoints);
            int outerAttempts = Mathf.RoundToInt(outerPoints.Count * outerSpawnProbability);
            for (int i = 0; i < outerAttempts; i++)
            {
                Vector2Int point = outerPoints[i];
                Vector3 candidatePos = new Vector3(point.x, 0, point.y);
                IslandData selectedIsland = SelectIslandFromBucket(outerRingBucket);

                if (IsPositionObstructed(candidatePos, selectedIsland.exclusionRadius))
                    continue;

                float height = Random.Range(minHeight, maxHeight);
                Vector3 finalPos = candidatePos + Vector3.up * height;
                SpawnIsland(selectedIsland, finalPos);
            }
        }

        private void SpawnIsland(IslandData data, Vector3 position)
        {
            if (data.prefab == null) return;

            // Apply random rotation
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            GameObject instance = Instantiate(data.prefab, position, rotation, this.transform);
            instance.name = $"{data.prefab.name}_{position}";
            GenerateTerrain(ref data.terrainData);
            if (startingIslandSet)
            {
                TerrainGenerator terrainGenerator = GetComponent<TerrainGenerator>();
                terrainGenerator.GenerateMap(data.terrainData, instance);
            }

            if (!startingIslandSet) startingIslandSet = true;
            TerrainNoiseData t = data.terrainData;

            if (_visibilityManager != null)
            {
                // If it's placed out in the empty gap or outer farlands
                if (new Vector3(position.x, 0, position.z).magnitude > innerRadius)
                {
                    _visibilityManager.RegisterFarlandsIsland(instance);
                }
            }

            // Decorate island with items, terrain features, and enemies (SERVER ONLY)
            // Decorations will be network-spawned and synced to clients
            if (data.decorationProfile != null)
            {
                if (isServer)
                {
                    // Save current random state before decoration
                    Random.State savedState = Random.state;

                    IslandDecorator decorator = instance.AddComponent<IslandDecorator>();
                    decorator.DecorateIsland(data.decorationProfile, data.exclusionRadius);

                    // Restore random state so server and clients stay in sync
                    Random.state = savedState;
                }
            }

            // Record placement
            placedIslands.Add(new PlacedIsland
            {
                position = position,
                radius = data.exclusionRadius,
                name = instance.name
            });
        }

        // Checks if a position is obstructed by another island's based on both exclusion radii
        private bool IsPositionObstructed(Vector3 pos, float newRadius)
        {
            foreach (var island in placedIslands)
            {
                // We check if the distance is less than EITHER the existing island's radius OR the new island's radius.
                // Effectively: Dist < Max(R_existing, R_new)
                // This ensures clear space defined by the largest requirement.
                float dist = Vector3.Distance(pos, island.position);
                if (dist < island.radius || dist < newRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetTotalWeight()
        {
            int sum = 0;
            if (individualIslands != null)
            {
                foreach (var island in individualIslands)
                {
                    sum += island.weight;
                }
            }

            if (islandBuckets != null)
            {
                foreach (var bucket in islandBuckets)
                {
                    sum += bucket.weight;
                }
            }

            return sum;
        }

        // Randomly selects an island based on its weight, picking either individual or from a bucket
        private IslandData SelectWeightedIsland(int totalWeight)
        {
            if (totalWeight <= 0)
            {
                return GetFallbackIsland();
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentSum = 0;

            if (individualIslands != null)
            {
                foreach (var island in individualIslands)
                {
                    currentSum += island.weight;
                    if (randomValue < currentSum)
                    {
                        return island;
                    }
                }
            }

            if (islandBuckets != null)
            {
                foreach (var bucket in islandBuckets)
                {
                    currentSum += bucket.weight;
                    if (randomValue < currentSum)
                    {
                        return SelectIslandFromBucket(bucket);
                    }
                }
            }

            return GetFallbackIsland();
        }

        private IslandData ApplyBucketExclusionRadius(IslandData island, float bucketRadius)
        {
            if (bucketRadius > 0f)
            {
                island.exclusionRadius = bucketRadius;
            }

            return island;
        }

        private IslandData ApplyBucketDefaults(IslandData island, IslandBucket bucket)
        {
            // Apply bucket exclusion radius if set
            if (bucket.exclusionRadius > 0f)
            {
                island.exclusionRadius = bucket.exclusionRadius;
            }

            // Apply weighted decoration profile selection if island doesn't have one
            if (island.decorationProfile == null && bucket.decorationProfiles != null &&
                bucket.decorationProfiles.Count > 0)
            {
                island.decorationProfile = SelectWeightedDecorationProfile(bucket.decorationProfiles);
            }

            return island;
        }

        private IslandDecorationProfile SelectWeightedDecorationProfile(
            List<IslandBucket.DecorationProfileEntry> profiles)
        {
            if (profiles == null || profiles.Count == 0) return null;

            int totalWeight = 0;
            foreach (var entry in profiles)
            {
                totalWeight += entry.weight;
            }

            if (totalWeight <= 0)
            {
                return profiles[Random.Range(0, profiles.Count)].profile;
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentSum = 0;

            foreach (var entry in profiles)
            {
                currentSum += entry.weight;
                if (randomValue < currentSum)
                {
                    return entry.profile;
                }
            }

            return profiles[Random.Range(0, profiles.Count)].profile;
        }

        private IslandData SelectIslandFromBucket(IslandBucket bucket)
        {
            if (bucket.islands == null || bucket.islands.Count == 0) return default;

            int bucketTotalWeight = 0;
            foreach (var island in bucket.islands)
            {
                bucketTotalWeight += island.weight;
            }

            if (bucketTotalWeight <= 0)
            {
                return ApplyBucketDefaults(bucket.islands[Random.Range(0, bucket.islands.Count)], bucket);
            }

            int randomValue = Random.Range(0, bucketTotalWeight);
            int currentSum = 0;
            foreach (var island in bucket.islands)
            {
                currentSum += island.weight;
                if (randomValue < currentSum)
                {
                    return ApplyBucketDefaults(island, bucket);
                }
            }

            return ApplyBucketDefaults(bucket.islands[Random.Range(0, bucket.islands.Count)], bucket);
        }

        private IslandData GetFallbackIsland()
        {
            if (individualIslands != null && individualIslands.Count > 0)
            {
                return individualIslands[Random.Range(0, individualIslands.Count)];
            }

            if (islandBuckets != null && islandBuckets.Count > 0 && islandBuckets[0].islands != null &&
                islandBuckets[0].islands.Count > 0)
            {
                return islandBuckets[0].islands[Random.Range(0, islandBuckets[0].islands.Count)];
            }

            return default;
        }

        // For shuffling the list of possible island generation points
        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // Not sure if this will actually be needed but it's here
        private void ClearWorld()
        {
            // Destroy all children of this transform
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            placedIslands.Clear();
        }

        private void GenerateTerrain(ref TerrainNoiseData terrainData)
        {
            terrainData.noiseScale = Random.Range(1.0f, 6.0f);
            terrainData.persistence = Random.Range(0f, 0f);
            terrainData.lacunarity = Random.Range(1f, 2f);
            terrainData.heightMeshMultiplier = Random.Range(.25f, 0.75f);
            terrainData.octaves = Random.Range(1, 4);
            terrainData.seed = Random.Range(0, int.MaxValue);
            terrainData.offset = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            terrainData.heightCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(1f, 1f)
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            foreach (var island in placedIslands)
            {
                Gizmos.DrawWireSphere(island.position, island.radius);
            }
        }
    }
}