using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    [RequireComponent(typeof(WorldGenerator))]
    public class BoundaryWindManager : MonoBehaviour
    {
        public static BoundaryWindManager Instance { get; private set; }

        public float maxInwardWindSpeed = 100f;
        public float maxClockwiseWindSpeed = 50f;

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
                    // Using square curve so it starts weak and becomes very strong at the end
                    float tFactor = Mathf.Clamp01((dist - innerRadius) / (outerRadius - innerRadius));
                    float strengthFactor = tFactor * tFactor;

                    Vector3 inwardDir = -pos2D.normalized;
                    Vector3 clockwiseDir = Vector3.Cross(Vector3.up, pos2D.normalized);

                    Vector3 wind = (inwardDir * maxInwardWindSpeed + clockwiseDir * maxClockwiseWindSpeed) * strengthFactor;

                    // Absolute barrier beyond outer radius
                    if (dist > outerRadius)
                    {
                        wind += inwardDir * (dist - outerRadius) * 200f;
                    }

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
