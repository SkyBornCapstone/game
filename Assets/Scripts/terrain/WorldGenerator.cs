using System.Collections.Generic;
using UnityEngine;
using ProceduralTerrain;
namespace Terrain
{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("World Settings")]
        [Tooltip("The size of the world grid (square).")]
        public int worldSize = 1000;
        
        [Tooltip("Probability of attempting to spawn an island at any given valid point (0.0 to 1.0).")]
        [Range(0f, 1f)]
        public float spawnProbability = 0.01f;

        [Header("Vertical Variation")]
        public float minHeight = -25f;
        public float maxHeight = 25f;

        [Header("Island Configuration")]
        [Tooltip("The specific island to always spawn at (0,0,0).")]
        public IslandData startingIsland;
        
        [Tooltip("List of individual island types to generate.")]
        public List<IslandData> individualIslands;

        [Tooltip("List of island buckets. A bucket is chosen first by its weight, then an island within it is chosen by its local weight.")]
        public List<IslandBucket> islandBuckets;

        // Internal list to track placed islands for exclusion checking
        private struct PlacedIsland
        {
            public Vector3 position;
            public float radius;
            public string name; // Debugging
        }
        
        private List<PlacedIsland> placedIslands = new List<PlacedIsland>();

        private bool startingIslandSet = false;
        // Trigger worldgen on start
        private void Start()
        {
            GenerateWorld();
        }

        [ContextMenu("Generate World")]
        public void GenerateWorld()
        {
            ClearWorld();

            placedIslands.Clear();
            List<Vector2Int> availablePoints = new List<Vector2Int>();

            // Center the grid of possible island spawnpoints around 0,0. Range is [-worldSize/2, worldSize/2]
            int halfSize = worldSize / 2;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int z = -halfSize; z <= halfSize; z++)
                {
                    availablePoints.Add(new Vector2Int(x, z));
                }
            }

            // Place Starting Island at (0,0)
            SpawnIsland(startingIsland, Vector3.zero);

            // Instead of looping through all points with a probability of spawning at each one,
            // we will shuffle the points and then trim the list to the number of attempts we want
            // Ex: instead of checking 100 points with a 10% spawn probability, we will check 10 random
            // points with a 100% spawn probability
            Shuffle(availablePoints);
            int totalWeight = GetTotalWeight();
            int attempts = Mathf.RoundToInt(availablePoints.Count * spawnProbability);
            for (int i = 0; i < attempts; i++)
            {
                Vector2Int point = availablePoints[i];

                // Convert grid point to world position (on plane y=0 initially)
                Vector3 candidatePos = new Vector3(point.x, 0, point.y);

                // Select Island Type
                IslandData selectedIsland = SelectWeightedIsland(totalWeight);

                // Check exclusion with the specific island's radius
                if (IsPositionObstructed(candidatePos, selectedIsland.exclusionRadius))
                {
                    continue; 
                }

                // Final Placement with height randomization
                float height = Random.Range(minHeight, maxHeight);
                Vector3 finalPos = candidatePos + Vector3.up * height;
                
                // Spawn Island handles rotation randomization
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

            if(!startingIslandSet ) startingIslandSet = true;
            TerrainNoiseData t = data.terrainData;
            print($"[{data.prefab.name}_{position}] seed={t.seed} scale={t.noiseScale:F2} persistence={t.persistence:F2} lacunarity={t.lacunarity:F2} heightMult={t.heightMeshMultiplier:F2} octaves={t.octaves} offset={t.offset}");
            // Record placement
            placedIslands.Add(new PlacedIsland { 
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
                return ApplyBucketExclusionRadius(bucket.islands[Random.Range(0, bucket.islands.Count)], bucket.exclusionRadius);
            }

            int randomValue = Random.Range(0, bucketTotalWeight);
            int currentSum = 0;
            foreach (var island in bucket.islands)
            {
                currentSum += island.weight;
                if (randomValue < currentSum)
                {
                    return ApplyBucketExclusionRadius(island, bucket.exclusionRadius);
                }
            }
            return ApplyBucketExclusionRadius(bucket.islands[Random.Range(0, bucket.islands.Count)], bucket.exclusionRadius);
        }

        private IslandData GetFallbackIsland()
        {
            if (individualIslands != null && individualIslands.Count > 0)
            {
                return individualIslands[Random.Range(0, individualIslands.Count)];
            }
            if (islandBuckets != null && islandBuckets.Count > 0 && islandBuckets[0].islands != null && islandBuckets[0].islands.Count > 0)
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