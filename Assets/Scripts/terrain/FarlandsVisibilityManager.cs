using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class FarlandsVisibilityManager : MonoBehaviour
    {
        public float visibilityThresholdRadius = 400f; 
        private readonly List<GameObject> _farlandsIslands = new List<GameObject>();
        private bool _isCurrentlyVisible = true;
        private Camera _mainCamera;

        public void RegisterFarlandsIsland(GameObject island)
        {
            _farlandsIslands.Add(island);
            // Match the current state instantly
            island.SetActive(_isCurrentlyVisible);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            UpdateVisibility();
        }

        private void Update()
        {
            // Update twice a second for performance since it's just a distance check
            if (Time.frameCount % 30 == 0)
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
            
            bool shouldBeVisible = camPos.magnitude > visibilityThresholdRadius;

            if (shouldBeVisible != _isCurrentlyVisible)
            {
                _isCurrentlyVisible = shouldBeVisible;
                for (int i = 0; i < _farlandsIslands.Count; i++)
                {
                    if (_farlandsIslands[i] != null)
                    {
                        _farlandsIslands[i].SetActive(_isCurrentlyVisible);
                    }
                }
            }
        }
    }
}
