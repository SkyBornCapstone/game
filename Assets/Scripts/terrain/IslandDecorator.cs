using System.Collections.Generic;
using Enemy;
using PurrNet;
using UnityEngine;
using UnityEngine.AI;

namespace Terrain
{
    public class IslandDecorator : NetworkBehaviour
    {
        private List<Vector3> spawnedPositions = new();
        private IslandDecorationProfile profile;
        private float islandRadius;

        public void DecorateIsland(IslandDecorationProfile decorationProfile, float radius)
        {
            if (decorationProfile == null) return;

            profile = decorationProfile;
            islandRadius = radius;
            spawnedPositions.Clear();

            // Spawn in order: terrain features first, then items, then enemies
            SpawnTerrainFeatures();
            SpawnItems();
            SpawnEnemies();
        }

        #region Item Spawning

        private void SpawnItems()
        {
            if (profile.items == null || profile.items.Count == 0) return;

            int attempts = Mathf.RoundToInt(profile.itemSpawnAttempts * profile.densityMultiplier);

            for (int i = 0; i < attempts; i++)
            {
                var entry = profile.items[Random.Range(0, profile.items.Count)];

                if (entry.itemPrefab == null) continue;

                if (Random.value > entry.spawnProbability) continue;

                Vector3? spawnPos = FindValidSpawnPosition(0f, 0f);
                if (spawnPos.HasValue)
                {
                    SpawnItem(entry.itemPrefab, spawnPos.Value);
                }
            }
        }

        private void SpawnItem(GameObject itemPrefab, Vector3 position)
        {
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject item = Instantiate(itemPrefab, position, rotation, null);
            spawnedPositions.Add(position);
        }

        #endregion

        #region Terrain Feature Spawning

        private void SpawnTerrainFeatures()
        {
            if (profile.terrainFeatures == null || profile.terrainFeatures.Count == 0) return;

            foreach (var entry in profile.terrainFeatures)
            {
                if (entry.preset == null || entry.preset.featurePrefab == null) continue;

                int spawnCount = Random.Range(entry.spawnCountRange.x, entry.spawnCountRange.y + 1);
                spawnCount = Mathf.RoundToInt(spawnCount * profile.densityMultiplier);

                for (int i = 0; i < spawnCount; i++)
                {
                    if (Random.value > entry.preset.spawnProbability) continue;

                    if (entry.preset.allowClustering)
                    {
                        SpawnFeatureCluster(entry.preset);
                    }
                    else
                    {
                        Vector3? spawnPos =
                            FindValidSpawnPosition(entry.preset.heightOffset, entry.preset.maxSlopeAngle);
                        if (spawnPos.HasValue)
                        {
                            SpawnTerrainFeature(entry.preset, spawnPos.Value);
                        }
                    }
                }
            }
        }

        private void SpawnFeatureCluster(TerrainFeaturePreset preset)
        {
            Vector3? centerPos = FindValidSpawnPosition(preset.heightOffset, preset.maxSlopeAngle);
            if (!centerPos.HasValue) return;

            int clusterSize = Random.Range(preset.clusterSizeRange.x, preset.clusterSizeRange.y + 1);

            for (int i = 0; i < clusterSize; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * preset.clusterRadius;
                Vector3 clusterPos = centerPos.Value + new Vector3(randomOffset.x, 0, randomOffset.y);

                if (Physics.Raycast(clusterPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
                {
                    if (IsSlopeValid(hit.normal, preset.maxSlopeAngle) && !IsPositionTooClose(hit.point))
                    {
                        SpawnTerrainFeature(preset, hit.point + Vector3.up * preset.heightOffset);
                    }
                }
            }
        }

        private void SpawnTerrainFeature(TerrainFeaturePreset preset, Vector3 position)
        {
            Quaternion rotation = CalculateRotation(preset.randomYRotation, preset.randomFullRotation,
                preset.alignToSurface, position);
            GameObject feature = Instantiate(preset.featurePrefab, position, rotation, null);

            if (preset.randomizeScale)
            {
                float scale = Random.Range(preset.scaleRange.x, preset.scaleRange.y);
                feature.transform.localScale *= scale;
            }

            spawnedPositions.Add(position);
        }

        #endregion

        #region Enemy Spawning

        private void SpawnEnemies()
        {
            if (profile.enemies == null || profile.enemies.Count == 0) return;

            foreach (var entry in profile.enemies)
            {
                if (entry.preset == null || entry.preset.enemyPrefab == null) continue;

                int spawnCount = Random.Range(entry.spawnCountRange.x, entry.spawnCountRange.y + 1);
                spawnCount = Mathf.RoundToInt(spawnCount * profile.densityMultiplier);

                for (int i = 0; i < spawnCount; i++)
                {
                    if (Random.value > entry.preset.spawnProbability) continue;

                    if (entry.preset.spawnInGroups)
                    {
                        SpawnEnemyGroup(entry.preset);
                    }
                    else
                    {
                        Vector3? spawnPos = FindValidSpawnPosition(entry.preset.heightOffset,
                            entry.preset.maxSlopeAngle, entry.preset.edgeBuffer);
                        if (spawnPos.HasValue && IsNavMeshValid(spawnPos.Value, entry.preset.requiresNavMesh))
                        {
                            SpawnEnemy(entry.preset, spawnPos.Value);
                        }
                    }
                }
            }
        }

        private void SpawnEnemyGroup(EnemySpawnPreset preset)
        {
            Vector3? centerPos = FindValidSpawnPosition(preset.heightOffset, preset.maxSlopeAngle, preset.edgeBuffer);
            if (!centerPos.HasValue || !IsNavMeshValid(centerPos.Value, preset.requiresNavMesh)) return;

            int groupSize = Random.Range(preset.groupSizeRange.x, preset.groupSizeRange.y + 1);

            for (int i = 0; i < groupSize; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * preset.groupRadius;
                Vector3 groupPos = centerPos.Value + new Vector3(randomOffset.x, 0, randomOffset.y);

                if (Physics.Raycast(groupPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
                {
                    if (IsSlopeValid(hit.normal, preset.maxSlopeAngle) &&
                        !IsPositionTooClose(hit.point) &&
                        IsNavMeshValid(hit.point, preset.requiresNavMesh))
                    {
                        SpawnEnemy(preset, hit.point + Vector3.up * preset.heightOffset);
                    }
                }
            }
        }

        private void SpawnEnemy(EnemySpawnPreset preset, Vector3 position)
        {
            Quaternion rotation = preset.randomYRotation
                ? Quaternion.Euler(0, Random.Range(0f, 360f), 0)
                : Quaternion.identity;
            GameObject enemy = Instantiate(preset.enemyPrefab, position, rotation, null);

            var enemyAI = enemy.GetComponent<BasicEnemyAi>();
            if (enemyAI != null)
            {
                enemyAI.walkPointRange = preset.patrolRadius;
            }

            spawnedPositions.Add(position);
        }

        #endregion

        #region Helper Methods

        private Vector3? FindValidSpawnPosition(float heightOffset, float maxSlope, float edgeBuffer = 0f)
        {
            for (int attempt = 0; attempt < profile.maxSpawnAttempts; attempt++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * (islandRadius - edgeBuffer);
                Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 10f, randomCircle.y);

                if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 20f))
                {
                    if (IsSlopeValid(hit.normal, maxSlope) && !IsPositionTooClose(hit.point))
                    {
                        return hit.point + Vector3.up * heightOffset;
                    }
                }
            }

            return null;
        }

        private bool IsSlopeValid(Vector3 normal, float maxSlope)
        {
            if (maxSlope <= 0f) return true;
            float angle = Vector3.Angle(Vector3.up, normal);
            return angle <= maxSlope;
        }

        private bool IsPositionTooClose(Vector3 position)
        {
            foreach (var pos in spawnedPositions)
            {
                if (Vector3.Distance(position, pos) < profile.minSpacing)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNavMeshValid(Vector3 position, bool requiresNavMesh)
        {
            if (!requiresNavMesh) return true;

            NavMeshHit navHit;
            return NavMesh.SamplePosition(position, out navHit, 2f, NavMesh.AllAreas);
        }

        private Quaternion CalculateRotation(bool randomY, bool randomFull, bool alignToSurface, Vector3 position)
        {
            Quaternion rotation = Quaternion.identity;

            if (alignToSurface && Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
            {
                rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            if (randomFull)
            {
                rotation *= Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            }
            else if (randomY)
            {
                rotation *= Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }

            return rotation;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (profile == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, islandRadius);

            Gizmos.color = Color.green;
            foreach (var pos in spawnedPositions)
            {
                Gizmos.DrawWireSphere(pos, profile.minSpacing);
            }
        }
    }
}