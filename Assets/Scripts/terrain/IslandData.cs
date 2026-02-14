using UnityEngine;

namespace Terrain
{
    [System.Serializable]
    public struct IslandData
    {
        public GameObject prefab;
        [Tooltip("Likelihood of this island being chosen.")]
        public int weight;
    }
}
