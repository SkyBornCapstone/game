using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    [RequireComponent(typeof(WorldGenerator))]
    public class BoundaryWindManager : MonoBehaviour
    {
        public static BoundaryWindManager Instance { get; private set; }

        public float maxInwardWindSpeed = 40f;
        public float maxClockwiseWindSpeed = 5f;

        private WorldGenerator _worldGenerator;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _worldGenerator = GetComponent<WorldGenerator>();
        }

        public Vector3 GetWindAtPosition(Vector3 position)
        {
            if (_worldGenerator == null) return Vector3.zero;

            float innerRadius = _worldGenerator.innerRadius;
            float outerRadius = innerRadius + _worldGenerator.emptyRingWidth;

            Vector3 pos2D = new Vector3(position.x, 0, position.z);
            float dist = pos2D.magnitude;

            if (dist > innerRadius)
            {
                // Scale from 0 at inner radius to 1 at outer radius
                float tFactor = Mathf.Clamp01((dist - innerRadius) / (outerRadius - innerRadius));
                
                // Inward wind scales quadratically (to give some space for clockwise movement)
                float inwardStrength = tFactor * tFactor;
                
                // Clockwise wind ramps up linearly
                float clockwiseStrength = tFactor;

                Vector3 inwardDir = -pos2D.normalized;
                Vector3 clockwiseDir = Vector3.Cross(Vector3.up, pos2D.normalized);

                return inwardDir * (maxInwardWindSpeed * inwardStrength) 
                     + clockwiseDir * (maxClockwiseWindSpeed * clockwiseStrength);
            }
            
            return Vector3.zero;
        }
    }
}
