using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class FarlandsVisibilityManager : MonoBehaviour
    {
        public float visibilityThresholdRadius = 400f; 
        public float randomOffsetRange = 100f; // Each island threshold offset
        public float fadeDistanceBand = 75f; // Distance over which the island rises
        public float riseDepth = 250f; // How far below standard elevation it starts

        private class IslandData
        {
            public GameObject Island;
            public float RandomThresholdOffset;
            public float OriginalY;
            public bool IsVisible;
        }

        private readonly List<IslandData> _farlandsIslands = new List<IslandData>();
        private Camera _mainCamera;

        public void RegisterFarlandsIsland(GameObject island)
        {
            var data = new IslandData
            {
                Island = island,
                RandomThresholdOffset = Random.Range(0f, randomOffsetRange),
                OriginalY = island.transform.position.y,
                IsVisible = false // Force update check to sync correctly
            };
            
            _farlandsIslands.Add(data);
            island.SetActive(false);
            
            // Set it to its hidden depth right away
            island.transform.position = new Vector3(island.transform.position.x, data.OriginalY - riseDepth, island.transform.position.z);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            UpdateVisibility();
        }

        private void Update()
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            Vector3 camPos = _mainCamera.transform.position;
            camPos.y = 0; // measure flat 2D distance to center of the world
            float currentDist = camPos.magnitude;

            for (int i = 0; i < _farlandsIslands.Count; i++)
            {
                var data = _farlandsIslands[i];
                if (data.Island == null) continue;

                float islandBaseThreshold = visibilityThresholdRadius + data.RandomThresholdOffset;
                float islandFullyVisibleTarget = islandBaseThreshold + fadeDistanceBand;
                
                // If it is entirely too far away
                if (currentDist < islandBaseThreshold)
                {
                    if (data.IsVisible)
                    {
                        data.IsVisible = false;
                        data.Island.SetActive(false);
                    }
                }
                else
                {
                    if (!data.IsVisible)
                    {
                        data.IsVisible = true;
                        data.Island.SetActive(true);
                    }

                    // Calculate depth
                    float depthLerp = Mathf.InverseLerp(islandBaseThreshold, islandFullyVisibleTarget, currentDist);
                    // Smoothstep for prettier rise
                    depthLerp = Mathf.SmoothStep(0f, 1f, depthLerp);
                    
                    float targetY = Mathf.Lerp(data.OriginalY - riseDepth, data.OriginalY, depthLerp);
                    
                    Vector3 islandPos = data.Island.transform.position;
                    // Only update if it actually moved significantly to save physics recalculation ticks if fully rested
                    if (Mathf.Abs(islandPos.y - targetY) > 0.01f)
                    {
                        islandPos.y = targetY;
                        data.Island.transform.position = islandPos;
                    }
                }
            }
        }
    }
}
