using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class FarlandsVisibilityManager : MonoBehaviour
    {
        public float visibilityThresholdRadius = 400f; 
        public float randomOffsetRange = 100f; // Each island will pop in progressively slower

        private class IslandData
        {
            public GameObject Island;
            public float RandomThresholdOffset;
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
                IsVisible = false // Force update check to sync correctly
            };
            
            _farlandsIslands.Add(data);
            island.SetActive(false);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            UpdateVisibility();
        }

        private void Update()
        {
            // Checking periodically to stagger the pop in calculations
            if (Time.frameCount % 10 == 0)
            {
                UpdateVisibility();
            }
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
            float distSquared = camPos.sqrMagnitude;
            float currentDist = Mathf.Sqrt(distSquared);

            for (int i = 0; i < _farlandsIslands.Count; i++)
            {
                var data = _farlandsIslands[i];
                if (data.Island == null) continue;

                bool shouldBeVisible = currentDist > (visibilityThresholdRadius + data.RandomThresholdOffset);

                if (shouldBeVisible != data.IsVisible)
                {
                    data.IsVisible = shouldBeVisible;
                    data.Island.SetActive(data.IsVisible);
                }
            }
        }
    }
}
