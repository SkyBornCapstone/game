using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    [RequireComponent(typeof(WorldGenerator))]
    public class BoundaryWindManager : MonoBehaviour
    {
        public static BoundaryWindManager Instance { get; private set; }

        public float maxInwardWindSpeed = 40f;
        public float maxClockwiseWindSpeed = 3f;

        private WorldGenerator _worldGenerator;
        private readonly List<IWindAffected> _windReceivers = new List<IWindAffected>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            _worldGenerator = GetComponent<WorldGenerator>();
        }

        public void Register(IWindAffected receiver)
        {
            if (!_windReceivers.Contains(receiver))
            {
                _windReceivers.Add(receiver);
            }
        }

        public void Unregister(IWindAffected receiver)
        {
            _windReceivers.Remove(receiver);
        }

        private void FixedUpdate()
        {
            if (_worldGenerator == null) return;

            float innerRadius = _worldGenerator.innerRadius;
            float outerRadius = innerRadius + _worldGenerator.emptyRingWidth;

            for (int i = 0; i < _windReceivers.Count; i++)
            {
                var receiver = _windReceivers[i];
                if (receiver == null || receiver.TransformRoot == null) continue;

                Vector3 pos = receiver.TransformRoot.position;
                Vector3 pos2D = new Vector3(pos.x, 0, pos.z);
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

                    Vector3 wind = inwardDir * (maxInwardWindSpeed * inwardStrength) 
                                 + clockwiseDir * (maxClockwiseWindSpeed * clockwiseStrength);

                    receiver.WindVelocity = wind;
                }
                else
                {
                    receiver.WindVelocity = Vector3.zero;
                }
            }
        }
    }
}
